import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})
export class AdminComponent implements OnInit {
  users: any[] = [];
  analytics: string = '';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.fetchUsers();
    this.fetchAnalytics();
  }

  fetchUsers(): void {
    this.http.get<any[]>('/api/admin/users').subscribe(
      (data) => {
        this.users = data;
      },
      (error) => {
        console.error('Error fetching users:', error);
      }
    );
  }

  fetchAnalytics(): void {
    this.http.get<{ analytics: string }>('/api/admin/analytics').subscribe(
      (data) => {
        this.analytics = data.analytics;
      },
      (error) => {
        console.error('Error fetching analytics:', error);
      }
    );
  }

  exportToCSV(): void {
    const csvData = this.users.map((user) =>
      `${user.id},${user.username},${user.email},${user.role}`
    );
    const csvContent = 'ID,Username,Email,Role\n' + csvData.join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.setAttribute('href', url);
    link.setAttribute('download', 'users.csv');
    link.click();
  }
}
