import { FormEvent, useEffect, useMemo, useState } from 'react';
import type { ReactElement } from 'react';
import {
  BadgeCheck,
  Boxes,
  Building2,
  ChevronRight,
  KeyRound,
  Laptop,
  LogOut,
  RefreshCw,
  Search,
  ShieldCheck,
  Users
} from 'lucide-react';

type Software = {
  id: string;
  machineName: string;
  userName: string;
  softwareName: string;
  version: string;
  publisher: string;
  installDate: string;
  isMicrosoftProduct: boolean;
  isWindowsProduct: boolean;
  isOfficeProduct: boolean;
  hasLicenseKeyAndActivated: boolean;
  collectionDate: string;
};

type LoginResponse = {
  token: string;
};

type FilterKey = 'todos' | 'microsoft' | 'office' | 'licenciados' | 'pendentes';

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '') ||
  'https://comfortable-gentleness-production-1da8.up.railway.app';
const TOKEN_STORAGE_KEY = 'softledger.token';

const filters: { key: FilterKey; label: string }[] = [
  { key: 'todos', label: 'Todos' },
  { key: 'microsoft', label: 'Microsoft' },
  { key: 'office', label: 'Office' },
  { key: 'licenciados', label: 'Ativados' },
  { key: 'pendentes', label: 'Pendentes' }
];

const microsoft365Catalog = [
  { name: 'Microsoft Teams', keywords: ['teams'] },
  { name: 'Outlook / Exchange', keywords: ['outlook', 'exchange'] },
  { name: 'OneDrive', keywords: ['onedrive'] },
  { name: 'SharePoint', keywords: ['sharepoint'] },
  { name: 'Word', keywords: ['word'] },
  { name: 'Excel', keywords: ['excel'] },
  { name: 'PowerPoint', keywords: ['powerpoint', 'power point'] },
  { name: 'Power BI', keywords: ['power bi', 'powerbi'] },
  { name: 'Microsoft Defender', keywords: ['defender'] },
  { name: 'Microsoft Copilot', keywords: ['copilot'] }
];

function App() {
  const [token, setToken] = useState(() => localStorage.getItem(TOKEN_STORAGE_KEY) || '');
  const [email, setEmail] = useState('admin@teste.com');
  const [password, setPassword] = useState('123456');
  const [software, setSoftware] = useState<Software[]>([]);
  const [query, setQuery] = useState('');
  const [activeFilter, setActiveFilter] = useState<FilterKey>('todos');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  async function login(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsLoading(true);
    setError('');

    try {
      const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
      });

      if (!response.ok) {
        throw new Error('Email ou senha inválidos.');
      }

      const data = (await response.json()) as LoginResponse;
      localStorage.setItem(TOKEN_STORAGE_KEY, data.token);
      setToken(data.token);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Não foi possível autenticar.');
    } finally {
      setIsLoading(false);
    }
  }

  async function loadSoftware(authToken = token) {
    if (!authToken) return;

    setIsLoading(true);
    setError('');

    try {
      const response = await fetch(`${API_BASE_URL}/Software`, {
        headers: { Authorization: `Bearer ${authToken}` }
      });

      if (response.status === 401) {
        localStorage.removeItem(TOKEN_STORAGE_KEY);
        setToken('');
        throw new Error('Sessão expirada. Faça login novamente.');
      }

      if (!response.ok) {
        throw new Error('Não foi possível carregar o inventário.');
      }

      const data = (await response.json()) as Software[];
      setSoftware(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao consultar a API.');
    } finally {
      setIsLoading(false);
    }
  }

  function logout() {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    setToken('');
    setSoftware([]);
  }

  useEffect(() => {
    if (token) {
      void loadSoftware(token);
    }
  }, [token]);

  const filteredSoftware = useMemo(() => {
    const normalizedQuery = normalize(query);

    return software.filter((item) => {
      const matchesQuery = [
        item.userName,
        item.machineName,
        item.softwareName,
        item.publisher,
        item.version
      ]
        .filter(Boolean)
        .some((value) => normalize(value).includes(normalizedQuery));

      const matchesFilter =
        activeFilter === 'todos' ||
        (activeFilter === 'microsoft' && item.isMicrosoftProduct) ||
        (activeFilter === 'office' && item.isOfficeProduct) ||
        (activeFilter === 'licenciados' && item.hasLicenseKeyAndActivated) ||
        (activeFilter === 'pendentes' && !item.hasLicenseKeyAndActivated);

      return matchesQuery && matchesFilter;
    });
  }, [activeFilter, query, software]);

  const metrics = useMemo(() => {
    const users = new Set(software.map((item) => fallback(item.userName, 'Sem usuario')));
    const machines = new Set(software.map((item) => fallback(item.machineName, 'Sem maquina')));
    const microsoftProducts = software.filter((item) => item.isMicrosoftProduct).length;
    const officeProducts = software.filter((item) => item.isOfficeProduct).length;
    const activated = software.filter((item) => item.hasLicenseKeyAndActivated).length;

    return {
      total: software.length,
      users: users.size,
      machines: machines.size,
      microsoftProducts,
      officeProducts,
      activated,
      pending: Math.max(software.length - activated, 0)
    };
  }, [software]);

  const peopleBoard = useMemo(() => {
    const groups = new Map<string, Software[]>();

    filteredSoftware.forEach((item) => {
      const key = fallback(item.userName, 'Sem usuario');
      groups.set(key, [...(groups.get(key) || []), item]);
    });

    return Array.from(groups.entries())
      .map(([userName, apps]) => ({
        userName,
        apps,
        machines: Array.from(new Set(apps.map((item) => fallback(item.machineName, 'Sem maquina')))),
        activated: apps.filter((item) => item.hasLicenseKeyAndActivated).length,
        microsoft: apps.filter((item) => item.isMicrosoftProduct).length,
        lastCollection: latestDate(apps.map((item) => item.collectionDate))
      }))
      .sort((a, b) => b.apps.length - a.apps.length);
  }, [filteredSoftware]);

  const m365Services = useMemo(() => {
    return microsoft365Catalog.map((service) => {
      const matches = software.filter((item) => {
        const haystack = normalize(`${item.softwareName} ${item.publisher}`);
        return service.keywords.some((keyword) => haystack.includes(keyword));
      });

      return {
        ...service,
        count: matches.length,
        users: new Set(matches.map((item) => fallback(item.userName, 'Sem usuario'))).size
      };
    });
  }, [software]);

  if (!token) {
    return (
      <main className="auth-shell">
        <section className="auth-panel">
          <div className="brand-mark">
            <Building2 size={28} aria-hidden="true" />
          </div>
          <div>
            <p className="eyebrow">SoftLedger</p>
            <h1>Board de apps e assinaturas</h1>
          </div>

          <form className="login-form" onSubmit={login}>
            <label>
              Email
              <input
                autoComplete="email"
                inputMode="email"
                onChange={(event) => setEmail(event.target.value)}
                required
                type="email"
                value={email}
              />
            </label>
            <label>
              Senha
              <input
                autoComplete="current-password"
                onChange={(event) => setPassword(event.target.value)}
                required
                type="password"
                value={password}
              />
            </label>
            {error && <p className="error-message">{error}</p>}
            <button className="primary-button" disabled={isLoading} type="submit">
              <KeyRound size={18} aria-hidden="true" />
              {isLoading ? 'Entrando...' : 'Entrar'}
            </button>
          </form>
        </section>
      </main>
    );
  }

  return (
    <main className="app-shell">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <span className="brand-mark small">
            <Building2 size={20} aria-hidden="true" />
          </span>
          <div>
            <strong>SoftLedger</strong>
            <span>Gestão de licenças</span>
          </div>
        </div>

        <nav className="sidebar-nav" aria-label="Filtros do inventário">
          {filters.map((filter) => (
            <button
              className={filter.key === activeFilter ? 'nav-item active' : 'nav-item'}
              key={filter.key}
              onClick={() => setActiveFilter(filter.key)}
              type="button"
            >
              <ChevronRight size={16} aria-hidden="true" />
              {filter.label}
            </button>
          ))}
        </nav>

        <button className="ghost-button sidebar-logout" onClick={logout} type="button">
          <LogOut size={18} aria-hidden="true" />
          Sair
        </button>
      </aside>

      <section className="workspace">
        <header className="topbar">
          <div>
            <p className="eyebrow">Inventário conectado</p>
            <h1>Aplicativos por funcionário</h1>
          </div>
          <div className="topbar-actions">
            <div className="search-box">
              <Search size={18} aria-hidden="true" />
              <input
                aria-label="Buscar por usuário, máquina, software, fornecedor ou versão"
                onChange={(event) => setQuery(event.target.value)}
                placeholder="Buscar inventário"
                value={query}
              />
            </div>
            <button className="icon-button" onClick={() => void loadSoftware()} title="Atualizar" type="button">
              <RefreshCw className={isLoading ? 'spin' : ''} size={18} aria-hidden="true" />
            </button>
          </div>
        </header>

        {error && <p className="error-message inline">{error}</p>}

        <section className="metric-grid" aria-label="Resumo do inventário">
          <Metric icon={<Boxes />} label="Apps mapeados" value={metrics.total} tone="cyan" />
          <Metric icon={<Users />} label="Funcionários" value={metrics.users} tone="green" />
          <Metric icon={<Laptop />} label="Máquinas" value={metrics.machines} tone="violet" />
          <Metric icon={<BadgeCheck />} label="Ativados" value={metrics.activated} tone="amber" />
          <Metric icon={<ShieldCheck />} label="Microsoft" value={metrics.microsoftProducts} tone="red" />
        </section>

        <section className="content-grid">
          <div className="board-column wide">
            <div className="section-heading">
              <div>
                <h2>Funcionários</h2>
                <span>{filteredSoftware.length} itens no filtro atual</span>
              </div>
            </div>

            <div className="people-grid">
              {peopleBoard.map((person) => (
                <article className="person-card" key={person.userName}>
                  <div className="person-header">
                    <div>
                      <h3>{person.userName}</h3>
                      <span>{person.machines.join(', ')}</span>
                    </div>
                    <strong>{person.apps.length}</strong>
                  </div>

                  <div className="status-row">
                    <span>{person.activated} ativados</span>
                    <span>{person.microsoft} Microsoft</span>
                    <span>{formatDate(person.lastCollection)}</span>
                  </div>

                  <div className="app-list">
                    {person.apps.slice(0, 6).map((item) => (
                      <div className="app-item" key={item.id}>
                        <div>
                          <strong>{fallback(item.softwareName, 'Software sem nome')}</strong>
                          <span>{fallback(item.publisher, 'Fornecedor não informado')}</span>
                        </div>
                        <StatusPill active={item.hasLicenseKeyAndActivated} />
                      </div>
                    ))}
                  </div>
                </article>
              ))}

              {!peopleBoard.length && (
                <div className="empty-state">
                  <Boxes size={32} aria-hidden="true" />
                  <strong>Nenhum item encontrado</strong>
                  <span>Ajuste a busca ou atualize a conexão com a API.</span>
                </div>
              )}
            </div>
          </div>

          <aside className="board-column">
            <div className="section-heading">
              <div>
                <h2>Microsoft 365</h2>
                <span>{metrics.officeProducts} itens Office detectados</span>
              </div>
            </div>

            <div className="m365-list">
              {m365Services.map((service) => (
                <div className="m365-item" key={service.name}>
                  <div>
                    <strong>{service.name}</strong>
                    <span>{service.users} usuários</span>
                  </div>
                  <span className={service.count > 0 ? 'availability active' : 'availability'}>
                    {service.count > 0 ? `${service.count} detectados` : 'Não detectado'}
                  </span>
                </div>
              ))}
            </div>

            <div className="license-panel">
              <h2>Licenças</h2>
              <div className="license-meter">
                <span style={{ width: `${percentage(metrics.activated, metrics.total)}%` }} />
              </div>
              <div className="license-counts">
                <strong>{metrics.activated} ativados</strong>
                <span>{metrics.pending} pendentes</span>
              </div>
            </div>
          </aside>
        </section>
      </section>
    </main>
  );
}

function Metric({
  icon,
  label,
  value,
  tone
}: {
  icon: ReactElement;
  label: string;
  value: number;
  tone: 'cyan' | 'green' | 'violet' | 'amber' | 'red';
}) {
  return (
    <article className={`metric-card ${tone}`}>
      <span className="metric-icon">{icon}</span>
      <div>
        <strong>{value}</strong>
        <span>{label}</span>
      </div>
    </article>
  );
}

function StatusPill({ active }: { active: boolean }) {
  return <span className={active ? 'status-pill active' : 'status-pill'}>{active ? 'Ativo' : 'Pendente'}</span>;
}

function fallback(value: string | undefined | null, fallbackValue: string) {
  return value?.trim() || fallbackValue;
}

function normalize(value: string | undefined | null) {
  return fallback(value, '').toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
}

function latestDate(values: string[]) {
  return values
    .filter(Boolean)
    .sort((a, b) => new Date(b).getTime() - new Date(a).getTime())[0];
}

function formatDate(value: string | undefined) {
  if (!value) return 'Sem coleta';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return 'Sem coleta';
  return new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' }).format(date);
}

function percentage(value: number, total: number) {
  if (!total) return 0;
  return Math.round((value / total) * 100);
}

export default App;
