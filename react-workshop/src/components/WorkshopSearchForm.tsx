import { type FormEvent, useState } from 'react'

type WorkshopSearchFormProps = {
  initialTerm?: string
  onSearch(term: string): void
}

export function WorkshopSearchForm({
  initialTerm = '',
  onSearch,
}: WorkshopSearchFormProps) {
  const [term, setTerm] = useState(initialTerm)

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    onSearch(term.trim())
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="flex flex-col sm:flex-row gap-3 mb-3"
    >
      <input
        className="flex-1 rounded-xl bg-white/5 ring-1 ring-white/10 px-4 py-2 placeholder:text-slate-500
                   focus:outline-none focus:ring-2 focus:ring-sky-500"
        placeholder="Buscar por título..."
        autoComplete="off"
        value={term}
        onChange={(e) => setTerm(e.target.value)}
      />
      <button
        type="submit"
        className="rounded-xl px-5 py-2 font-medium bg-sky-600 hover:bg-sky-500 transition
                   shadow-[0_8px_30px_rgb(2,132,199,0.35)]"
      >
        Buscar
      </button>
    </form>
  )
}
