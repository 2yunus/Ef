import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { UserManagementComponent } from './user-management/user-management.component';

@Component({
  selector: 'app-admin-dashboard',
  template: `
    <div class="container mt-4">
      <div class="card">
        <div class="card-header">
          <h5 class="mb-0">Admin Dashboard</h5>
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

          <div *ngIf="!isLoading && !error && data" class="row">
            <div class="col-md-4 mb-4">
              <div class="card">
                <div class="card-body">
                  <h5 class="card-title">Total Users</h5>
                  <h2 class="display-4">{{ data.totalUsers }}</h2>
                </div>
              </div>
            </div>

            <div class="col-md-4 mb-4">
              <div class="card">
                <div class="card-body">
                  <h5 class="card-title">Total Feedback</h5>
                  <h2 class="display-4">{{ data.totalFeedback }}</h2>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <app-user-management></app-user-management>
    </div>
  `,
  standalone: true,
  imports: [CommonModule, UserManagementComponent]
})
export class AdminDashboardComponent implements OnInit {
  data: any = null;
  isLoading = false;
  error: string | null = null;

  constructor(
    private http: HttpClient,
    private router: Router
  ) {}

  ngOnInit(): void {
    const token = localStorage.getItem('token');
    if (!token) {
      this.router.navigate(['/login']);
      return;
    }
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.error = null;

    const token = localStorage.getItem('token');
    if (!token) {
      this.error = 'Not authenticated';
      this.isLoading = false;
      return;
    }

    this.http.get(`${environment.apiUrl}/api/admin/dashboard`, {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    }).subscribe({
      next: (response) => {
        console.log('Response:', response);
        this.data = response;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error:', error);
        if (error.status === 401) {
          this.router.navigate(['/login']);
        } else {
          this.error = 'Failed to load dashboard data';
        }
        this.isLoading = false;
      }
    });
  }
} 