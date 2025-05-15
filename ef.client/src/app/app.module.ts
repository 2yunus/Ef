import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { RegisterComponent } from './register/register.component';
import { LoginComponent } from './login/login.component';
import { ManagerDashboardComponent } from './manager/manager-dashboard.component';

export interface User {
  id: number;
  username: string;
  email: string;
}

export interface Feedback {
  id: number;
  content: string;
  category: string;
  sentiment: 'Positive' | 'Neutral' | 'Negative';
  createdAt: string;
  isAnonymous: boolean;
  user?: User;
}

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    ReactiveFormsModule,
    RegisterComponent,
    LoginComponent,
    ManagerDashboardComponent
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
