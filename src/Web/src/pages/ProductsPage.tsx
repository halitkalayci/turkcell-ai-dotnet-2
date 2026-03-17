import { useEffect, useState } from 'react'
import './ProductsPage.css'

interface Product {
  id: string
  name: string
  price: number
  stock: number
  sku: string
  createdAt: string
  updatedAt: string | null
}

type PageState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'success'; products: Product[] }

function ProductsPage() {
  const [state, setState] = useState<PageState>({ status: 'loading' })

  useEffect(() => {
    fetch('/api/v1/products')
      .then((res) => {
        if (!res.ok) throw new Error(`HTTP ${res.status}`)
        return res.json() as Promise<Product[]>
      })
      .then((products) => setState({ status: 'success', products }))
      .catch((err: unknown) =>
        setState({
          status: 'error',
          message: err instanceof Error ? err.message : 'Bilinmeyen hata',
        }),
      )
  }, [])

  return (
    <div className="products-page">
      <header className="products-header">
        <h1>Ürünler</h1>
      </header>

      {state.status === 'loading' && (
        <p className="products-status">Yükleniyor...</p>
      )}

      {state.status === 'error' && (
        <p className="products-status products-status--error">
          Hata: {state.message}
        </p>
      )}

      {state.status === 'success' && state.products.length === 0 && (
        <p className="products-status">Ürün bulunamadı.</p>
      )}

      {state.status === 'success' && state.products.length > 0 && (
        <ul className="products-grid">
          {state.products.map((product) => (
            <li key={product.id} className="product-card">
              <div className="product-card__sku">{product.sku}</div>
              <h2 className="product-card__name">{product.name}</h2>
              <div className="product-card__footer">
                <span className="product-card__price">
                  {product.price.toLocaleString('tr-TR', {
                    style: 'currency',
                    currency: 'TRY',
                  })}
                </span>
                <span className="product-card__stock">
                  Stok: {product.stock}
                </span>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}

export default ProductsPage
