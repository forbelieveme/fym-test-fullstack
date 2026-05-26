import { useEffect, useState } from "react";
import { usersApi, rolesApi, isSuperAdmin, extractError, type AppUser, type Role } from "../api";
import { useAuth } from "../auth";

export default function Users() {
  const { user: me } = useAuth();
  const [users, setUsers] = useState<AppUser[]>([]);
  const [roles, setRoles] = useState<Role[]>([]);
  const [err, setErr] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  // Create form state
  const canCreate = isSuperAdmin(me);
  const [newUserName, setNewUserName] = useState("");
  const [newEmail, setNewEmail] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [newRoleIds, setNewRoleIds] = useState<string[]>([]);
  const [createBusy, setCreateBusy] = useState(false);

  // Assign-role state per row
  const [assignFor, setAssignFor] = useState<string | null>(null);
  const [assignRoleId, setAssignRoleId] = useState("");

  async function refresh() {
    setLoading(true);
    setErr(null);
    try {
      const [u, r] = await Promise.all([usersApi.list(), rolesApi.list()]);
      setUsers(u);
      setRoles(r);
    } catch (e) {
      setErr(extractError(e));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    refresh();
  }, []);

  async function createUser(e: React.FormEvent) {
    e.preventDefault();
    setCreateBusy(true);
    setErr(null);
    try {
      await usersApi.create({
        userName: newUserName,
        email: newEmail,
        password: newPassword,
        roleIds: newRoleIds,
      });
      setNewUserName("");
      setNewEmail("");
      setNewPassword("");
      setNewRoleIds([]);
      await refresh();
    } catch (e) {
      setErr(extractError(e));
    } finally {
      setCreateBusy(false);
    }
  }

  async function assignRole(userId: string) {
    if (!assignRoleId) return;
    setErr(null);
    try {
      await usersApi.assignRoles(userId, [assignRoleId]);
      setAssignFor(null);
      setAssignRoleId("");
      await refresh();
    } catch (e) {
      setErr(extractError(e));
    }
  }

  return (
    <div>
      <h2>Users</h2>
      {err && <p className="error">{err}</p>}
      {loading ? (
        <p>Loading…</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Username</th>
              <th>Email</th>
              <th>Roles</th>
              <th>Created</th>
              {canCreate && <th>Actions</th>}
            </tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id}>
                <td>{u.userName}</td>
                <td>{u.email}</td>
                <td>{u.roles.map((r) => r.name).join(", ") || "—"}</td>
                <td>{new Date(u.createdAt).toLocaleString()}</td>
                {canCreate && (
                  <td>
                    {assignFor === u.id ? (
                      <span>
                        <select value={assignRoleId} onChange={(e) => setAssignRoleId(e.target.value)}>
                          <option value="">— role —</option>
                          {roles
                            .filter((r) => !u.roles.some((ur) => ur.id === r.id))
                            .map((r) => (
                              <option key={r.id} value={r.id}>
                                {r.name}
                              </option>
                            ))}
                        </select>
                        <button onClick={() => assignRole(u.id)} disabled={!assignRoleId}>
                          Save
                        </button>
                        <button onClick={() => setAssignFor(null)}>Cancel</button>
                      </span>
                    ) : (
                      <button onClick={() => setAssignFor(u.id)}>+ role</button>
                    )}
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {canCreate && (
        <section className="card">
          <h3>Create user (SuperAdmin)</h3>
          <form onSubmit={createUser}>
            <label>
              Username
              <input value={newUserName} onChange={(e) => setNewUserName(e.target.value)} required minLength={3} />
            </label>
            <label>
              Email
              <input type="email" value={newEmail} onChange={(e) => setNewEmail(e.target.value)} required />
            </label>
            <label>
              Password
              <input
                type="password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                required
                minLength={8}
              />
            </label>
            <fieldset>
              <legend>Roles</legend>
              {roles.map((r) => (
                <label key={r.id} className="inline">
                  <input
                    type="checkbox"
                    checked={newRoleIds.includes(r.id)}
                    onChange={(e) =>
                      setNewRoleIds((prev) =>
                        e.target.checked ? [...prev, r.id] : prev.filter((x) => x !== r.id)
                      )
                    }
                  />
                  {r.name}
                </label>
              ))}
            </fieldset>
            <button disabled={createBusy}>{createBusy ? "Creating…" : "Create user"}</button>
          </form>
        </section>
      )}
    </div>
  );
}
