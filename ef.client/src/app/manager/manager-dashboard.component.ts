import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { FeedbackService } from '../services/feedback.service';
import { Feedback } from '../models/feedback.model';

@Component({
  selector: 'app-manager-dashboard',
  templateUrl: './manager-dashboard.component.html',
  styleUrls: ['./manager-dashboard.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule]
})
export class ManagerDashboardComponent implements OnInit {
  feedbacks: Feedback[] = [];
  isLoading: boolean = true;
  errorMessage: string = '';

  constructor(
    private feedbackService: FeedbackService,
    private router: Router
  ) {}

  ngOnInit() {
    const token = localStorage.getItem('token');
    if (!token) {
      this.router.navigate(['/login']);
      return;
    }
    this.loadFeedbacks();
  }

  loadFeedbacks() {
    this.isLoading = true;
    this.errorMessage = '';

    this.feedbackService.getFeedbacks().subscribe({
      next: (data) => {
        console.log('Feedbacks loaded:', data);
        this.feedbacks = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading feedbacks:', error);
        if (error.status === 401) {
          this.router.navigate(['/login']);
        } else {
          this.errorMessage = 'Failed to load feedbacks. Please try again.';
        }
        this.isLoading = false;
      }
    });
  }
} 