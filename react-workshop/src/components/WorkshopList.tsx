import type { Workshop } from '../api/workshops'
import { WorkshopCard } from './WorkshopCard'

type WorkshopListProps = {
  items: Workshop[]
  loading: boolean
  error: string | null
}

export function WorkshopList({ items, loading, error }: WorkshopListProps) {
  if (loading) {
    return (
      <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 mt-4">
        {Array.from({ length: 6 }).map((_, i) => (
          <div
            key={i}
            className="h-28 rounded-2xl bg-white/5 ring-1 ring-white/10 animate-pulse"
          />
        ))}
      </div>
    )
  }

  if (error) {
    return (
      <div className="mt-4 text-sm text-red-400">
        Ocorreu um erro ao carregar os workshops: {error}
      </div>
    )
  }

  if (!items.length) {
    return (
      <p className="mt-4 text-slate-400 text-sm">
        Nenhum workshop encontrado para o termo informado.
      </p>
    )
  }

  return (
    <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 mt-4">
      {items.map((w) => (
        <WorkshopCard key={w.id} workshop={w} />
      ))}
    </div>
  )
}
