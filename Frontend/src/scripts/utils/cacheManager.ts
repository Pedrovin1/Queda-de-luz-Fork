interface CacheStruct<T> {
  value: T
  expire: number
}

//Cache reativo com o tempo TTL padrão 7 dias
// Evita manter informações não atualizadas de locais
export const cacheManager = {
  set: <T>(key: string, value: T, ttl: number = 7): void => {
    const expire = Date.now() + ttl * 24 * 60 * 60 * 1000
    const entry: CacheStruct<T> = { value, expire }
    localStorage.setItem(key, JSON.stringify(entry))
  },

  get: <T>(key: string): T | null => {
    const entryString = localStorage.getItem(key)
    if (!entryString) return null

    try {
      const entry: CacheStruct<T> = JSON.parse(entryString)

      if (Date.now() > entry.expire) {
        localStorage.removeItem(key)
        console.warn(`Cache expirado para chave: ${key}`)
        return null
      }
      return entry.value
    } catch (error) {
      localStorage.removeItem(key)
      return null
    }
  },
}
