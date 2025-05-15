import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { User } from '../../models/user.model';

@Component({
  selector: 'app-user-management',
  template: `
    <div class="card mt-4">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h5 class="mb-0">User Management</h5>
        <button class="btn btn-primary" (click)="exportToCsv()">
          <i class="bi bi-download"></i> Export to CSV
        </button>
      </div>
      <div class="card-body">
        <div *ngIf="isLoading" class="text-center">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>

        <div *ngIf="error" class="alert alert-danger" role="alert">
          {{ error }}
        </div>

        <div *ngIf="!isLoading && !error" class="table-responsive">
          <table class="table table-striped">
            <thead>
              <tr>
                <th>ID</th>
                <th>Username</th>
                <th>Email</th>
                <th>Role</th>
                <th>Created At</th>
                <th>Last Login</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let user of users">
                <td>{{ user.id }}</td>
                <td>{{ user.username }}</td>
                <td>{{ user.email }}</td>
                <td>
                  <span [ngClass]="{
                    'badge bg-primary': user.role === 'Admin',
                    'badge bg-success': user.role === 'Manager',
                    'badge bg-secondary': user.role === 'User'
                  }">
                    {{ user.role }}
                  </span>
                </td>
                <td>{{ user.createdAt | date:'medium' }}</td>
                <td>{{ user.lastLogin ? (user.lastLogin | date:'medium') : 'Never' }}</td>
                <td>
                  <button class="btn btn-sm btn-outline-primary me-2" (click)="editUser(user)">
                    <i class="bi bi-pencil"></i>
                  </button>
                  <button class="btn btn-sm btn-outline-danger" (click)="deleteUser(user)">
                    <i class="bi bi-trash"></i>
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `,
  standalone: true,
  imports: [CommonModule]
})
export class UserManagementComponent implements OnInit {
  users: User[] = [];
  isLoading = false;
  error: string | null = null;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.error = null;

    const token = localStorage.getItem('token');
    if (!token) {
      this.error = 'Not authenticated';
      this.isLoading = false;
      return;
    }

    this.http.get<User[]>(`${environment.apiUrl}/api/admin/users`, {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    }).subscribe({
      next: (response) => {
        console.log('Users loaded:', response);
        this.users = response;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading users:', error);
        this.error = 'Failed to load users';
        this.isLoading = false;
      }
    });
  }

  exportToCsv(): void {
    const headers = ['ID', 'Username', 'Email', 'Role', 'Created At', 'Last Login'];
    const csvData = this.users.map(user => [
      user.id,
      user.username,
      user.email,
      user.role,
      new Date(user.createdAt).toLocaleString(),
      user.lastLogin ? new Date(user.lastLogin).toLocaleString() : 'Never'
    ]);

    const csvContent = [
      headers.join(','),
      ...csvData.map(row => row.join(','))
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', `users_${new Date().toISOString().split('T')[0]}.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  editUser(user: User): void {
    // TODO: Implement edit functionality
    console.log('Edit user:', user);
  }

  deleteUser(user: User): void {
    // TODO: Implement delete functionality
    console.log('Delete user:', user);
  }
} 