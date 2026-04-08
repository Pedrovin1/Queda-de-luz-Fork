const requestHeader = {
  Accept: 'application/json',
  'X-Requested-With': 'XMLHttpRequest',
  'User-Agent': 'OutageMap/0.1 (https://github.com/NtanBraga/Queda-de-luz)',
}

//Requisição client-side para APIs de terceiros como Nominatim e Overpass
//Evitar bloqueio 'Too many attempts' em host-side caso haja muitas requisições

const rateLimit = async (): Promise<void> => {
  let lastRequest = 0
  const interval = 1100
  const date = Date.now()
  const timeSinceLastRequest = date - lastRequest

  if (timeSinceLastRequest < interval) {
    const waitTime = interval - timeSinceLastRequest
    await new Promise((resolve) => setTimeout(resolve, waitTime))
  }
  lastRequest = Date.now()
}

export const safeFetch = async (url: string, timeout = 25000): Promise<Response> => {
  if (url.includes('nominatim.openstreetmap.org')) await rateLimit()

  const controller = new AbortController()
  const timer = setTimeout(() => controller.abort, timeout)

  try {
    const response = await fetch(url, {
      headers: requestHeader,
      signal: controller.signal,
    })

    if (!response.ok) {
      throw new Error(`Erro na requisição da API: ${response.status}: ${response.statusText}`)
    }

    clearTimeout(timer)
    return response
  } catch (error: any) {
    clearTimeout(timer)
    if (error.name === 'AbortError') {
      throw new Error('A requisição demorou demmais: Timeout')
    }
    throw error
  }
}
