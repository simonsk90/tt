// ─── State ───────────────────────────────────────────────────────────────────
let eventSource = null;
let isRunning = false;

// ─── SSE Connection ───────────────────────────────────────────────────────────
function connectSSE() {
  if (eventSource) return;
  eventSource = new EventSource('/api/events');

  eventSource.onmessage = (e) => {
    const entry = JSON.parse(e.data);
    appendLogEntry(entry);

    if (entry.category === '✅ Complete' || entry.message.includes('Job complete')) {
      showComplete();
    }
  };

  eventSource.onerror = () => {
    // Reconnect handled by browser automatically
  };
}

// ─── Button Handler ───────────────────────────────────────────────────────────
async function startFieldMarking() {
  if (isRunning) return;

  const btn = document.getElementById('startBtn');
  const statusMsg = document.getElementById('statusMsg');

  isRunning = true;
  btn.disabled = true;
  statusMsg.classList.add('hidden');

  clearTerminalPlaceholder();
  connectSSE();

  appendLogEntry({ category: 'Command', message: 'StartFieldMarkingCommand → POST /api/robots/start', timestamp: new Date().toISOString() });

  try {
    const resp = await fetch('/api/robots/start', { method: 'POST' });
    const data = await resp.json();

    if (!resp.ok) {
      appendLogEntry({ category: 'Error', message: data.error ?? 'Unknown error', timestamp: new Date().toISOString() });
      resetButton();
    } else {
      appendLogEntry({ category: 'Command', message: `Accepted — robot ID: ${data.robotId}`, timestamp: new Date().toISOString() });
    }
  } catch (err) {
    appendLogEntry({ category: 'Error', message: `Network error: ${err.message}`, timestamp: new Date().toISOString() });
    resetButton();
  }
}

// ─── DOM Helpers ──────────────────────────────────────────────────────────────
function appendLogEntry(entry) {
  const terminal = document.getElementById('terminal');
  clearTerminalPlaceholder();

  const ts = new Date(entry.timestamp).toLocaleTimeString('en-GB', {
    hour: '2-digit', minute: '2-digit', second: '2-digit'
  });

  const tagClass = resolveTagClass(entry.category);

  const row = document.createElement('div');
  row.className = 'log-entry';
  row.innerHTML = `
    <span class="log-ts">${ts}</span>
    <span class="log-tag ${tagClass}">${escHtml(entry.category)}</span>
    <span class="log-msg">${escHtml(entry.message)}</span>
  `;

  terminal.appendChild(row);
  terminal.scrollTop = terminal.scrollHeight;
}

function resolveTagClass(category) {
  if (!category) return 'default';
  const c = category.toLowerCase();
  if (c.includes('command'))                     return 'Command';
  if (c.includes('domain event') || c === 'domain') return 'Domain';
  if (c.includes('event handler') || c === 'event handler') return 'Handler';
  if (c.includes('background job') || c === 'job') return 'Job';
  if (c.includes('complete') || c.includes('✅')) return 'Complete';
  return 'default';
}

function showComplete() {
  const statusMsg = document.getElementById('statusMsg');
  statusMsg.textContent = 'Job complete! 🚀';
  statusMsg.classList.remove('hidden');
  resetButton();
}

function resetButton() {
  isRunning = false;
  const btn = document.getElementById('startBtn');
  btn.disabled = false;
}

function clearTerminalPlaceholder() {
  const terminal = document.getElementById('terminal');
  const placeholder = terminal.querySelector('.terminal-placeholder');
  if (placeholder) placeholder.remove();
}

function clearLog() {
  const terminal = document.getElementById('terminal');
  terminal.innerHTML = '<div class="terminal-placeholder">Log cleared. Ready.</div>';
  document.getElementById('statusMsg').classList.add('hidden');
}

function escHtml(str) {
  return String(str ?? '')
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

// ─── Auto-connect SSE on page load to capture any in-progress runs ────────────
connectSSE();
