import { useState } from 'react'
import type { Workshop } from './api/workshops'
import { searchWorkshops } from './api/workshops'
import { WorkshopSearchForm } from './components/WorkshopSearchForm'
import { WorkshopList } from './components/WorkshopList'

function App() {
  const [items, setItems] = useState<Workshop[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [term, setTerm] = useState('')

  async function handleSearch(nextTerm: string) {
    setTerm(nextTerm)
    setError(null)

    if (!nextTerm) {
      setItems([])
      return
    }

    try {
      setLoading(true)
      const data = await searchWorkshops(nextTerm)
      setItems(data)
    } catch (err) {
      console.error(err)
      setError('Falha ao buscar workshops. Verifique o console/Network.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <main className="min-h-screen bg-gradient-to-br from-slate-950 via-slate-900 to-slate-950 text-slate-100 antialiased">
      <div className="max-w-6xl mx-auto px-4 py-12">
        <header className="mb-8">
          <h1 className="text-3xl font-semibold tracking-tight">Workshops</h1>
          <p className="text-slate-400 mt-1">
            React + TypeScript + Vite consumindo a API existente.
          </p>
        </header>

        <section className="backdrop-blur-md bg-white/5 ring-1 ring-white/10 rounded-2xl p-6 shadow-lg mb-8">
          <h2 className="text-xl font-semibold mb-2">Buscar Workshops</h2>

          <WorkshopSearchForm initialTerm={term} onSearch={handleSearch} />

          <p className="text-sm text-slate-400">
            {term
              ? `Mostrando resultados para “${term}”`
              : 'Digite um termo para buscar.'}
          </p>

          <WorkshopList items={items} loading={loading} error={error} />
        </section>

        {/* A próxima seção (Parte 2) será o formulário de criação */}
        <section className="backdrop-blur-md bg-white/5 ring-1 ring-white/10 rounded-2xl p-6 shadow-lg">
          <h2 className="text-xl font-semibold mb-4">
            Cadastrar novo Workshop
          </h2>
          <p className="text-sm text-slate-400">
            Implementaremos o formulário na Parte 2 do exercício.
          </p>
        </section>
      </div>
    </main>
  )
}

export default App
