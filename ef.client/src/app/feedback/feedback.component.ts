import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FeedbackService } from '../services/feedback.service';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-feedback',
  templateUrl: './feedback.component.html',
  styleUrls: ['./feedback.component.css'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule]
})
export class FeedbackComponent implements OnInit {
  feedbackForm: FormGroup;
  errorMessage: string = '';
  isSubmitting: boolean = false;
  categories = [
    { value: 'workplace', label: 'Workplace Environment' },
    { value: 'team', label: 'Team Collaboration' },
    { value: 'management', label: 'Management' },
    { value: 'process', label: 'Processes & Procedures' },
    { value: 'other', label: 'Other' }
  ];

  sentiments = [
    { value: 'Positive', label: 'Positive' },
    { value: 'Neutral', label: 'Neutral' },
    { value: 'Negative', label: 'Negative' }
  ];

  constructor(
    private fb: FormBuilder,
    private feedbackService: FeedbackService,
    private router: Router,
    private authService: AuthService
  ) {
    this.feedbackForm = this.fb.group({
      content: ['', [Validators.required, Validators.minLength(10)]],
      category: ['', Validators.required],
      sentiment: ['', Validators.required],
      isAnonymous: [false]
    });
  }

  ngOnInit() {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
    }
  }

  onSubmit() {
    if (this.feedbackForm.valid && !this.isSubmitting) {
      this.isSubmitting = true;
      this.errorMessage = '';
      this.feedbackForm.disable();

      console.log('Submitting feedback:', this.feedbackForm.value);

      this.feedbackService.submitFeedback(this.feedbackForm.value).subscribe({
        next: (response) => {
          console.log('Feedback submitted successfully:', response);
          this.feedbackForm.reset();
          this.feedbackForm.enable();
          this.isSubmitting = false;
          window.location.reload();
        },
        error: (error) => {
          console.error('Feedback submission error:', error);
          this.errorMessage = error.error?.message || 'Failed to submit feedback. Please try again.';
          this.isSubmitting = false;
          this.feedbackForm.enable();
        }
      });
    } else {
      this.markFormGroupTouched(this.feedbackForm);
    }
  }

  private markFormGroupTouched(formGroup: FormGroup) {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }
} 