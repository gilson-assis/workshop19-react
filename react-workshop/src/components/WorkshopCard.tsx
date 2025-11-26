import type { Workshop } from '../api/workshops'

type WorkshopCardProps = {
  workshop: Workshop
}

export function WorkshopCard({ workshop }: WorkshopCardProps) {
  const where = workshop.isOnline
    ? 'Online'
    : workshop.location || 'Presencial'

  const start = new Date(workshop.startAt)
  const end = new Date(workshop.endAt)
  const dateRange = `${start.toLocaleString()} — ${end.toLocaleString()}`

  return (
    <article className="rounded-2xl bg-white/5 ring-1 ring-white/10 p-4 shadow-md hover:bg-white/10 transition">
      <div className="flex items-center justify-between mb-2">
        <span className="text-xs px-2 py-0.5 rounded-full bg-slate-800 text-slate-200 ring-1 ring-white/10">
          {where}
        </span>
      </div>
      <h3 className="text-lg font-medium mb-1">{workshop.title}</h3>
      <p className="text-sm text-slate-400">{dateRange}</p>
    </article>
  )
}
