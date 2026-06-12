import { useEffect, useState, type ReactNode } from "react";
import { fetchObjectUrl } from "@/shared/api/files";

/**
 * <img> para arquivos protegidos: o endpoint exige Bearer, então uma tag <img src>
 * direta não funciona. Busca o blob autenticado, exibe via object URL e o revoga ao
 * desmontar/trocar de src. Enquanto carrega (ou em falha) mostra o `fallback`.
 *
 * O estado guarda o `src` ao qual a URL pertence: quando o src muda, o object URL
 * antigo deixa de bater e o fallback reaparece até o novo carregar — sem precisar
 * resetar estado de forma síncrona dentro do efeito.
 */
export function AuthImage({
  src,
  alt,
  className,
  fallback = null,
}: {
  src: string;
  alt: string;
  className?: string;
  fallback?: ReactNode;
}) {
  const [loaded, setLoaded] = useState<{ src: string; url: string } | null>(null);

  useEffect(() => {
    let cancelled = false;
    let created: string | null = null;

    fetchObjectUrl(src)
      .then((url) => {
        if (cancelled) {
          URL.revokeObjectURL(url);
          return;
        }
        created = url;
        setLoaded({ src, url });
      })
      .catch(() => {
        /* falha de carga: mantém o fallback */
      });

    return () => {
      cancelled = true;
      if (created) URL.revokeObjectURL(created);
    };
  }, [src]);

  if (loaded?.src === src) {
    return <img src={loaded.url} alt={alt} className={className} />;
  }
  return <>{fallback}</>;
}
