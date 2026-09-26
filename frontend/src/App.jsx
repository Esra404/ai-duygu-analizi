import { useState, useEffect, useRef } from 'react'
import './App.css'

// Environment variable'dan API URL'ini al, yoksa localhost kullan
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000'

function App() {
  const [username, setUsername] = useState('')
  const [currentUser, setCurrentUser] = useState('')
  const [message, setMessage] = useState('')
  const [messages, setMessages] = useState([])
  const [loading, setLoading] = useState(false)
  const [users, setUsers] = useState([])
  const messagesEndRef = useRef(null)

  useEffect(() => {
    if (currentUser) {
      loadMessages()
      loadUsers()
      // Her 2 saniyede bir mesajları yenile
      const interval = setInterval(() => {
        loadMessages()
        loadUsers()
      }, 2000)
      return () => clearInterval(interval)
    }
  }, [currentUser])

  useEffect(() => {
    scrollToBottom()
  }, [messages])

  const scrollToBottom = () => {
    // Kısa bir gecikme ile scroll yap (DOM güncellemesi için)
    setTimeout(() => {
      messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
    }, 100)
  }

  const handleLogin = () => {
    if (username.trim()) {
      setCurrentUser(username.trim())
      localStorage.setItem('chatUsername', username.trim())
    }
  }

  const handleLogout = () => {
    setCurrentUser('')
    setUsername('')
    localStorage.removeItem('chatUsername')
  }

  useEffect(() => {
    const savedUsername = localStorage.getItem('chatUsername')
    if (savedUsername) {
      setCurrentUser(savedUsername)
      setUsername(savedUsername)
    }
  }, [])

  const loadMessages = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/chat/history`)
      if (response.ok) {
        const data = await response.json()
        setMessages(data)
      } else {
        console.error('Mesajlar yüklenemedi:', response.status, response.statusText)
      }
    } catch (error) {
      console.error('Mesajlar yüklenirken hata:', error)
      // Backend çalışmıyor olabilir
      if (error.message.includes('Failed to fetch') || error.message.includes('NetworkError')) {
        console.warn('Backend bağlantısı kurulamadı. Backend çalışıyor mu kontrol edin.')
      }
    }
  }

  const loadUsers = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/users`)
      if (response.ok) {
        const data = await response.json()
        setUsers(Array.isArray(data) ? data : (data.users || []))
      }
    } catch (error) {
      console.error('Kullanıcılar yüklenirken hata:', error)
    }
  }

  const sendMessage = async (e) => {
    e.preventDefault()
    if (!message.trim() || !currentUser) return
    if (loading) return // Çift tıklamayı önle

    setLoading(true)
    const messageToSend = message.trim()
    setMessage('') // Hemen input'u temizle (UX iyileştirmesi)
    
    try {
      console.log('Mesaj gönderiliyor:', messageToSend)
      const response = await fetch(`${API_BASE_URL}/api/chat`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          username: currentUser,
          message: messageToSend,
        }),
      })

      console.log('Response status:', response.status)

      if (response.ok) {
        const data = await response.json()
        console.log('Mesaj başarıyla gönderildi:', data)
        setMessages((prev) => [...prev, data])
        loadUsers()
        loadMessages() // Mesaj listesini yenile
      } else {
        let errorText = `${response.status} ${response.statusText}`
        try {
          const errorData = await response.json()
          errorText += `\n${errorData.error || JSON.stringify(errorData)}`
        } catch {
          errorText += `\n(Detay alınamadı)`
        }
        console.error('Mesaj gönderme hatası:', errorText)
        alert(`Mesaj gönderilemedi:\n${errorText}\n\nBackend console'unu kontrol edin.`)
        setMessage(messageToSend) // Mesajı geri koy
      }
    } catch (error) {
      console.error('Mesaj gönderilirken exception:', error)
      if (error.message.includes('Failed to fetch') || error.message.includes('NetworkError')) {
        alert('❌ Backend bağlantısı kurulamadı!\n\nBackend\'in çalıştığından emin olun:\n1. Backend terminal\'ini kontrol edin\n2. http://localhost:5000 adresine tarayıcıdan erişmeyi deneyin')
      } else {
        alert(`❌ Hata: ${error.message}\n\nBackend console'unu kontrol edin.`)
      }
      setMessage(messageToSend) // Mesajı geri koy
    } finally {
      setLoading(false)
    }
  }

  if (!currentUser) {
    return (
      <div className="login-container">
        <div className="login-box">
          <h1>🤖 AI Duygu Analizi Chat</h1>
          <p>Chat'e katılmak için kullanıcı adınızı girin</p>
          <form onSubmit={handleLogin}>
            <input
              type="text"
              placeholder="Kullanıcı adınız..."
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="username-input"
              autoFocus
            />
            <button type="submit" className="login-button">
              Giriş Yap
            </button>
          </form>
        </div>
      </div>
    )
  }

  return (
    <div className="chat-container">
      <div className="chat-header">
        <h1>🤖 AI Duygu Analizi Chat</h1>
        <div className="user-info">
          <span className="current-user">Kullanıcı: {currentUser}</span>
          <button onClick={handleLogout} className="logout-button">
            Çıkış
          </button>
        </div>
      </div>

      <div className="users-sidebar">
        <h3>💬 Aktif Kullanıcılar ({users.length})</h3>
        <div className="users-list">
          {users.map((user, index) => (
            <div
              key={index}
              className={`user-item ${user === currentUser ? 'current' : ''}`}
            >
              {user === currentUser ? '👤 ' : '👥 '}
              {user}
            </div>
          ))}
        </div>
      </div>

      <div className="chat-main">
        <div className="messages-container">
          <div className="messages-wrapper">
            {messages.length === 0 ? (
              <div className="empty-state">
                <p>Henüz mesaj yok. İlk mesajı sen gönder! 💬</p>
              </div>
            ) : (
              messages.map((msg) => (
                <div
                  key={msg.id}
                  className={`message-wrapper ${
                    msg.username === currentUser ? 'own-message' : ''
                  }`}
                >
                  <div className="message-bubble">
                    <div className="message-header">
                      <span className="message-username">
                        {msg.username === currentUser ? 'Sen' : msg.username}
                      </span>
                      <span className="message-time">
                        {new Date(msg.timestamp).toLocaleTimeString('tr-TR', {
                          hour: '2-digit',
                          minute: '2-digit',
                        })}
                      </span>
                    </div>
                    <div className="message-text">{msg.message}</div>
                    <div className="ai-response">
                      <span className="ai-label">🤖 AI Cevabı:</span>
                      <div className="ai-text">{msg.response}</div>
                    </div>
                  </div>
                </div>
              ))
            )}
            <div ref={messagesEndRef} />
          </div>
        </div>

        <form onSubmit={sendMessage} className="message-form">
          <input
            type="text"
            placeholder="Mesajınızı yazın..."
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            className="message-input"
            disabled={loading}
          />
          <button
            type="submit"
            className="send-button"
            disabled={loading || !message.trim()}
          >
            {loading ? '⏳' : '📤'}
          </button>
        </form>
      </div>
    </div>
  )
}

export default App

