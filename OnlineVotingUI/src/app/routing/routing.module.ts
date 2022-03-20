import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { HomeContentComponent } from '../components/homepage/home-content/home-content.component';
import { LoginComponent } from '../components/homepage/home-menu/login/login.component';
import { RegisterComponent } from '../components/homepage/home-menu/register/register.component';
import { AboutComponent } from '../components/homepage/home-menu/about/about.component';
import { NotFoundComponent } from '../components/error-pages/not-found/not-found.component';



const routes: Routes = [
  { path: 'home', component: HomeContentComponent},
  { path: 'login', component: LoginComponent},
  { path: 'register', component: RegisterComponent},
  { path: 'about', component: AboutComponent},
  { path: '404', component : NotFoundComponent},
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: '**', redirectTo: '/404', pathMatch: 'full'}
];


@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    RouterModule.forRoot(routes)
  ],
  exports: [
    RouterModule
  ],
})
export class RoutingModule { }
