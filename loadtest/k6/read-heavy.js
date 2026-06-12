import http from "k6/http";
import { check } from "k6";

// ---------------------------------------------------------------------------
// Cenário de leitura multi-tenant. Exercita os endpoints de leitura mais quentes
// (listas com cursor, busca ILike, ficha, ocupação) distribuindo a carga entre os
// tenants semeados (ponderado por porte: tenants grandes recebem mais tráfego).
//
// Pré-requisito: rodar o seeder antes (gera ../manifest.json com token por tenant).
// Uso:  k6 run loadtest/k6/read-heavy.js
//       BASE_URL=http://localhost:5131 k6 run loadtest/k6/read-heavy.js
// ---------------------------------------------------------------------------

const BASE = __ENV.BASE_URL || "http://localhost:5131";
const manifest = JSON.parse(open("../manifest.json"));

// Pondera o pool de tenants por porte: SaaS real → tenants maiores geram mais requests.
const TIER_WEIGHT = { large: 6, medium: 2, small: 1 };
const POOL = [];
for (const t of manifest.tenants) {
  const weight = TIER_WEIGHT[t.tier] || 1;
  for (let i = 0; i < weight; i++) POOL.push(t);
}

// Termos curtos para forçar o caminho de busca por substring (ILike '%x%').
const SEARCH_TERMS = ["a", "jo", "ma", "re", "li", "to", "be", "an", "ca", "lu"];

const pick = (arr) => arr[Math.floor(Math.random() * arr.length)];

export const options = {
  scenarios: {
    read_heavy: {
      executor: "ramping-vus",
      startVUs: 0,
      stages: [
        { duration: "30s", target: 20 }, // aquecimento
        { duration: "1m", target: 50 },
        { duration: "1m", target: 100 }, // pico
        { duration: "30s", target: 0 }, // ramp-down
      ],
      gracefulStop: "10s",
    },
  },
  // Falha o teste se os SLOs estourarem (CI-friendly).
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<300", "p(99)<800"],
    "http_req_duration{name:list_pets}": ["p(95)<300"],
    "http_req_duration{name:search_pets}": ["p(95)<500"], // hotspot: ILike sem índice trigram
    "http_req_duration{name:list_tutors}": ["p(95)<300"],
    "http_req_duration{name:pet_detail}": ["p(95)<200"],
    "http_req_duration{name:pet_health}": ["p(95)<300"],
    "http_req_duration{name:occupancy}": ["p(95)<400"], // hotspot: overlap de datas sem índice
  },
};

export default function () {
  const tenant = pick(POOL);
  const params = (name) => ({
    headers: { Authorization: `Bearer ${tenant.token}` },
    tags: { name },
  });

  const r = Math.random();
  let res;

  if (r < 0.3) {
    res = http.get(`${BASE}/v1/pets?limit=20`, params("list_pets"));
  } else if (r < 0.45) {
    res = http.get(`${BASE}/v1/pets?search=${pick(SEARCH_TERMS)}&limit=20`, params("search_pets"));
  } else if (r < 0.6) {
    res = http.get(`${BASE}/v1/tutors?limit=20`, params("list_tutors"));
  } else if (r < 0.74) {
    res = http.get(`${BASE}/v1/pets/${pick(tenant.petIds)}`, params("pet_detail"));
  } else if (r < 0.87) {
    // usa healthPetIds (pets que têm ficha) para não gerar 404 ruidoso
    const ids = tenant.healthPetIds.length ? tenant.healthPetIds : tenant.petIds;
    res = http.get(`${BASE}/v1/pets/${pick(ids)}/health`, params("pet_health"));
  } else {
    res = http.get(
      `${BASE}/v1/occupancy?from=${tenant.occupancyFrom}&to=${tenant.occupancyTo}`,
      params("occupancy"),
    );
  }

  check(res, { "status 200": (r) => r.status === 200 });
}
