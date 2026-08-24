// Zastita rute: proverava da li token postoji u localStorage.
// Ako ne postoji, odmah preusmerava na Verify stranicu i vraca false
// - pozivalac (entry fajl) tad ne sme da renderuje zasticenu stranicu.
export function requireAuth(): boolean {
  if (!localStorage.getItem('accessToken')) {
    window.location.href = '/index.html'
    return false
  }

  return true
}
