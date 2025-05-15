import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Feedback } from '../models/feedback.model';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';
import { tap, map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class FeedbackService {
  private apiUrl = `${environment.apiUrl}/api/feedback`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  private getHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders().set('Authorization', `Bearer ${token}`);
  }

  submitFeedback(feedback: Feedback): Observable<any> {
    console.log('Submitting feedback:', feedback);
    return this.http.post(this.apiUrl, feedback, { headers: this.getHeaders() });
  }

  getTeamFeedback(): Observable<Feedback[]> {
    return this.http.get<Feedback[]>(`${this.apiUrl}/team`, { headers: this.getHeaders() });
  }

  getAnalytics(): Observable<any> {
    return this.http.get(`${this.apiUrl}/analytics`, { headers: this.getHeaders() });
  }

  getFeedbacks(): Observable<Feedback[]> {
    console.log('Getting feedbacks from:', `${this.apiUrl}`);
    console.log('Headers:', this.getHeaders().keys());
    
    return this.http.get<Feedback[]>(`${this.apiUrl}`, { 
      headers: this.getHeaders(),
      observe: 'response'
    }).pipe(
      tap(response => {
        console.log('Full response:', response);
        console.log('Response headers:', response.headers);
        console.log('Response status:', response.status);
        console.log('Response body:', response.body);
      }),
      map(response => response.body || [])
    );
  }

  getFeedbackById(id: number): Observable<Feedback> {
    return this.http.get<Feedback>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }

  updateFeedback(id: number, feedback: Feedback): Observable<Feedback> {
    return this.http.put<Feedback>(`${this.apiUrl}/${id}`, feedback, { headers: this.getHeaders() });
  }

  deleteFeedback(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }
} 