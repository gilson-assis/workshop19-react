## ⚛️ Workshop React — Exercícios Práticos (2h)

**Objetivo:** Refatorar a tela de Workshops (busca + criação) para **React + TypeScript**, utilizando **Vite** e **Tailwind CSS**, consumindo a API de workshops existente: [http://workshop.verum.tech/swagger/index.html](http://workshop.verum.tech/swagger/index.html).

-----

## PARTE 1 — Setup e Busca de Workshops

### 1\. Setup do Projeto

1.  **Criar projeto React + TypeScript com Vite:**

    ```bash
    # 1.1 Criar projeto
    npm create vite@latest react-workshops -- --template react-ts
    cd react-workshops

    # 1.2 Instalar dependências básicas
    npm install

    # 1.3 Instalar axios (para HTTP) e Tailwind CSS (via plugin do Vite)
    npm install axios
    npm install -D tailwindcss @tailwindcss/vite
    ```

2.  **Configurar Tailwind CSS**

      * Edite `vite.config.ts` para habilitar o plugin:

        ```typescript
        import { defineConfig } from 'vite'
        import react from '@vitejs/plugin-react'
        import tailwindcss from '@tailwindcss/vite'

        // https://vitejs.dev/config/
        export default defineConfig({
          plugins: [
            react(),
            tailwindcss(), // habilita Tailwind v4 via plugin Vite
          ],
        })
        ```

      * Edite `src/index.css`, adicionando o import do tailwind no início:

        ```css
        @import "tailwindcss";

        /* Adicione o restante do seu CSS aqui */
        ```

3.  **Configurar URL da API**

      * Crie um arquivo `.env` na raiz do projeto:

        ```env
        VITE_API_BASE="http://localhost:5298/api"
        VITE_API_TOKEN="<gerar o token na API>"
        ```

4.  **Tipos e Funções de API**

      * Crie o diretório: `mkdir -p src/api`

      * Crie `src/api/workshops.ts`:

        ```typescript
        import axios from 'axios'

        export type Workshop = {
          id: string
          title: string
          startAt: string   // ISO string
          endAt: string     // ISO string
          isOnline: boolean
          location?: string | null
          capacity?: number
        }
        export type WorkshopList = Workshop[]

        const API_BASE = import.meta.env.VITE_API_BASE ?? 'http://localhost:5000'
        const TOKEN = import.meta.env.VITE_API_TOKEN ?? ''
        const WORKSHOPS_PATH = '/workshops' // Ajuste se sua API for /api/workshops, por exemplo

        const getHeaders = () => {
            return {
                Authorization: `Bearer ${TOKEN}`
            }
        }

        export async function searchWorkshops(term: string): Promise<WorkshopList> {
          const q = term.trim()
          const { data } = await axios.get<WorkshopList>(
            `${API_BASE}${WORKSHOPS_PATH}`,
            { params: { q }, headers: getHeaders() }   // se sua API não usa "q", ajuste aqui
          )
          return data
        }
        ```

5.  **Componentes de Lista (WorkshopCard e WorkshopList)**

      * Crie o diretório: `mkdir -p src/components`

      * Crie `src/components/WorkshopCard.tsx`:

        ```typescript
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
        ```

      * Crie `src/components/WorkshopList.tsx`:

        ```typescript
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
        ```

6.  **Formulário de Busca (WorkshopSearchForm)**

      * Crie `src/components/WorkshopSearchForm.tsx`:

        ```typescript
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
        ```

7.  **Componente Principal (App.tsx)**

      * Substitua o conteúdo de `src/App.tsx`:

        ```typescript
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
        ```

8.  **Entrypoint (`main.tsx`)**

      * Confira se `src/main.tsx` está correto (geralmente não precisa de alteração):

        ```typescript
        import React from 'react'
        import ReactDOM from 'react-dom/client'
        import App from './App.tsx'
        import './index.css'

        ReactDOM.createRoot(document.getElementById('root')!).render(
          <React.StrictMode>
            <App />
          </React.StrictMode>,
        )
        ```

9.  **Ajustar Interface (CSS)**

      * No arquivo `src/App.css`, dentro de `#root`, **remova** a configuração `max-width: 1280px;`.
      * No arquivo `src/index.css`, dentro de `body`, **remova** as configurações `display: flex;` e `place-items: center;`.

10. **Rodar e Testar**

    ```bash
    npm run dev
    ```

    Acesse a URL (ex.: `http://localhost:5173`) e teste a busca.

-----

## PARTE 2 — Formulário de Inserção de Workshop em React

Implementação da função de criação e do formulário controlado para inserir novos workshops.

### 1\. Estender a API — `createWorkshop`

  * Edite `src/api/workshops.ts` e adicione os tipos e a função `createWorkshop`:

    ```typescript
    // ... imports, Workshop type, WorkshopList type, constants, getHeaders ...

    export type NewWorkshop = {
      title: string
      startAt: string   // ISO
      endAt: string     // ISO
      isOnline: boolean
      location?: string | null
      capacity?: number
    }

    // ... searchWorkshops function ...

    export async function createWorkshop(payload: NewWorkshop): Promise<Workshop> {
      const { data } = await axios.post<Workshop>(
        `${API_BASE}${WORKSHOPS_PATH}`,
        payload,
        { headers: getHeaders()}
      )
      return data
    }
    ```

### 2\. Componente `NewWorkshopForm`

  * Crie `src/components/NewWorkshopForm.tsx`:

    ```typescript
    import { type FormEvent, useState } from 'react'
    import type { NewWorkshop, Workshop } from '../api/workshops'
    import { createWorkshop } from '../api/workshops'

    type NewWorkshopFormProps = {
      onCreated(workshop: Workshop): void
    }

    function toIsoFromLocal(input: string): string {
      if (!input) throw new Error('Data/hora inválida.')
      const d = new Date(input)
      if (isNaN(d.getTime())) throw new Error('Formato de data/hora inválido.')
      return d.toISOString() // converte para UTC ISO
    }

    export function NewWorkshopForm({ onCreated }: NewWorkshopFormProps) {
      const [title, setTitle] = useState('')
      const [capacity, setCapacity] = useState('')
      const [isOnline, setIsOnline] = useState(false)
      const [location, setLocation] = useState('')
      const [startAt, setStartAt] = useState('')
      const [endAt, setEndAt] = useState('')
      const [submitting, setSubmitting] = useState(false)
      const [error, setError] = useState<string | null>(null)
      const [success, setSuccess] = useState<string | null>(null)

      function buildPayload(): NewWorkshop {
        const trimmedTitle = title.trim()
        if (trimmedTitle.length < 5) {
          throw new Error('Título deve ter pelo menos 5 caracteres.')
        }

        const startIso = toIsoFromLocal(startAt)
        const endIso = toIsoFromLocal(endAt)
        const startMs = new Date(startIso).getTime()
        const endMs = new Date(endIso).getTime()
        if (endMs <= startMs) {
          throw new Error('EndAt deve ser depois de StartAt.')
        }

        let loc: string | null | undefined = null
        if (!isOnline) {
          const trimmedLoc = location.trim()
          if (!trimmedLoc) {
            throw new Error('Local é obrigatório para workshops presenciais.')
          }
          loc = trimmedLoc
        }

        let cap: number | undefined
        if (capacity.trim()) {
          const n = Number(capacity)
          if (!Number.isFinite(n) || n < 1) {
            throw new Error('Capacidade deve ser um número >= 1.')
          }
          cap = n
        }

        const payload: NewWorkshop = {
          title: trimmedTitle,
          startAt: startIso,
          endAt: endIso,
          isOnline,
          location: loc,
          capacity: cap,
        }

        return payload
      }

      async function handleSubmit(e: FormEvent) {
        e.preventDefault()
        setError(null)
        setSuccess(null)

        try {
          const payload = buildPayload()
          setSubmitting(true)
          const created = await createWorkshop(payload)
          setSuccess('Workshop inserido com sucesso!')
          // Limpar o formulário
          setTitle('')
          setCapacity('')
          setIsOnline(false)
          setLocation('')
          setStartAt('')
          setEndAt('')
          // Notificar o componente pai
          onCreated(created)
        } catch (err) {
          console.error(err)
          const msg = err instanceof Error ? err.message : 'Erro ao inserir'
          setError(msg)
        } finally {
          setSubmitting(false)
        }
      }

      return (
        <form
          onSubmit={handleSubmit}
          className="grid gap-3 md:grid-cols-2"
          noValidate
        >
          <input
            id="title"
            name="title"
            className="rounded-xl bg-white/5 ring-1 ring-white/10 px-4 py-2"
            placeholder="Título"
            required
            value={title}
            onChange={(e) => setTitle(e.target.value)}
          />

          <input
            id="capacity"
            name="capacity"
            type="number"
            min={1}
            className="rounded-xl bg-white/5 ring-1 ring-white/10 px-4 py-2"
            placeholder="Capacidade (opcional)"
            value={capacity}
            onChange={(e) => setCapacity(e.target.value)}
          />

          <label className="flex items-center gap-2">
            <input
              id="isOnline"
              name="isOnline"
              type="checkbox"
              className="accent-sky-500"
              checked={isOnline}
              onChange={(e) => setIsOnline(e.target.checked)}
            />
            <span>Online</span>
          </label>

          <input
            id="location"
            name="location"
            className="rounded-xl bg-white/5 ring-1 ring-white/10 px-4 py-2"
            placeholder="Local (se presencial)"
            value={location}
            onChange={(e) => setLocation(e.target.value)}
          />

          <input
            id="startAt"
            name="startAt"
            type="datetime-local"
            className="rounded-xl bg-white/5 ring-1 ring-white/10 px-4 py-2"
            required
            value={startAt}
            onChange={(e) => setStartAt(e.target.value)}
          />

          <input
            id="endAt"
            name="endAt"
            type="datetime-local"
            className="rounded-xl bg-white/5 ring-1 ring-white/10 px-4 py-2"
            required
            value={endAt}
            onChange={(e) => setEndAt(e.target.value)}
          />

          <button
            type="submit"
            disabled={submitting}
            className="md:col-span-2 mt-2 rounded-xl px-5 py-2 font-medium
                       bg-emerald-600 hover:bg-emerald-500 disabled:opacity-60 transition"
          >
            {submitting ? 'Enviando...' : 'Inserir'}
          </button>

          {error && (
            <p className="md:col-span-2 text-sm text-red-400 mt-1">{error}</p>
          )}
          {success && (
            <p className="md:col-span-2 text-sm text-emerald-400 mt-1">
              {success}
            </p>
          )}
        </form>
      )
    }
    ```

### 3\. Integrar o formulário no `App.tsx`

  * Edite `src/App.tsx` para importar e usar o `NewWorkshopForm`, incluindo a lógica `handleCreated`:

    ```typescript
    import { useState } from 'react'
    import type { Workshop } from './api/workshops'
    import { searchWorkshops } from './api/workshops'
    import { WorkshopSearchForm } from './components/WorkshopSearchForm'
    import { WorkshopList } from './components/WorkshopList'
    import { NewWorkshopForm } from './components/NewWorkshopForm' // Novo import

    function App() {
      // ... estados existentes ...
      const [items, setItems] = useState<Workshop[]>([])
      const [loading, setLoading] = useState(false)
      const [error, setError] = useState<string | null>(null)
      const [term, setTerm] = useState('')

      // ... handleSearch existente ...
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

      function handleCreated(created: Workshop) {
        // Se o novo workshop se encaixar no filtro atual, adiciona na frente da lista
        if (!term || created.title.toLowerCase().includes(term.toLowerCase())) {
          setItems((prev) => [created, ...prev])
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

            <section className="backdrop-blur-md bg-white/5 ring-1 ring-white/10 rounded-2xl p-6 shadow-lg">
              <h2 className="text-xl font-semibold mb-4">
                Cadastrar novo Workshop
              </h2>
              {/* Novo componente de formulário */}
              <NewWorkshopForm onCreated={handleCreated} /> 
            </section>
          </div>
        </main>
      )
    }
    export default App
    ```

### 4\. Testar o Fluxo Completo

```bash
npm run dev
```

  * **Busca:** Verifique o *loading*, a listagem e as mensagens de erro.
  * **Inserção:** Preencha o formulário, valide as mensagens de sucesso/erro e confira se o novo workshop aparece na lista (se o filtro de busca permitir).

-----

## PARTE 3 — Desafio (Opcional)

Escolha um ou mais desafios para estender a funcionalidade.

### Desafio A — Filtros Avançados de Workshops

  * **Ideia:** Adicionar filtros por **modalidade** e **ordenação por data**.

  * **Tipos:**

    ```typescript
    type ModeFilter = 'all' | 'online' | 'onsite'
    type SortOrder = 'asc' | 'desc'
    ```

  * **Sugestão de Implementação:**

    1.  Adicionar os estados `modeFilter` e `sortOrder` em `<App />`.
    2.  Criar os componentes de UI (selects ou botões) para manipular esses estados na seção de busca.
    3.  Aplicar a **filtragem** e **ordenação** no array `items` retornado por `searchWorkshops` antes de passá-lo para `<WorkshopList />`.

### Desafio B — Ver Detalhes em um Modal

  * **Ideia:** Abrir um modal com detalhes completos ao clicar em um `WorkshopCard`.

  * **Sugestão de Implementação:**

    1.  Adicionar o estado `selectedWorkshop: Workshop | null` em `<App />`.
    2.  Modificar `<WorkshopCard />` para receber um prop `onClick` e estilizar como clicável.
    3.  Em `<WorkshopList />`, repassar uma função `onSelect` que chame `setSelectedWorkshop` no `App`.
    4.  Criar o componente `<WorkshopModal />` para exibir os detalhes quando `selectedWorkshop` não for nulo.

### Desafio C — Manter o Termo de Busca e Filtros no URL

  * **Ideia:** Sincronizar o estado da busca (`term`, `modeFilter`, `sortOrder`) com o *query string* da URL para permitir compartilhamento.

  * **Sugestão de Implementação:**

    1.  Em `<App />`, usar `new URLSearchParams(window.location.search)` na montagem inicial para ler os valores de `q`, `mode` e `sort`, e usá-los como **estado inicial**.
    2.  Usar um `useEffect` no `App` que observe as mudanças em `term`, `modeFilter` e `sortOrder`.
    3.  Dentro do `useEffect`, construir o novo *query string* e usar `window.history.replaceState(null, '', newUrl)` para atualizar a URL sem recarregar a página.
