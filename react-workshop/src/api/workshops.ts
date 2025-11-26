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
const WORKSHOPS_PATH = '/workshops'

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
