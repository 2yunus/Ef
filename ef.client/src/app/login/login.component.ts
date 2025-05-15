import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth.service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule]
})
export class LoginComponent {
  loginForm: FormGroup;
  errorMessage: string = '';
  isSubmitting: boolean = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private http: HttpClient
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.loginForm.valid && !this.isSubmitting) {
      this.isSubmitting = true;
      this.errorMessage = '';
      this.loginForm.disable();

      const credentials = {
        email: this.loginForm.get('email')?.value,
        password: this.loginForm.get('password')?.value
      };

      console.log('Sending login request with credentials:', credentials);

      this.authService.login(credentials).subscribe({
        next: (response: any) => {
          console.log('Login response:', response);
          if (response && response.token) {
            const role = this.authService.getUserRole();
            console.log('Decoded user role:', role);
            
            // Redirect based on role with exact comparison
            if (role === 'Admin') {
              console.log('Redirecting to admin dashboard');
              this.router.navigate(['/admin']);
            } else if (role === 'Manager') {
              console.log('Redirecting to manager dashboard');
              this.router.navigate(['/manager']);
            } else {
              console.log('Redirecting to feedback page');
              this.router.navigate(['/feedback']);
            }
          } else {
            console.error('No token in response:', response);
            this.errorMessage = 'Invalid response from server';
            this.isSubmitting = false;
            this.loginForm.enable();
          }
        },
        error: (error) => {
          console.error('Login error details:', {
            status: error.status,
            statusText: error.statusText,
            error: error.error,
            message: error.message
          });
          this.errorMessage = error.error?.message || 'Login failed. Please try again.';
          this.isSubmitting = false;
          this.loginForm.enable();
        }
      });
    } else {
      this.markFormGroupTouched(this.loginForm);
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