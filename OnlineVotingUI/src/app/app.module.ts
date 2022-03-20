import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule } from './material/material.module';
import { RoutingModule } from './routing/routing.module';
import {FlexLayoutModule} from '@angular/flex-layout';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';


import { AppComponent } from './app.component';
import { HomeLayoutComponent } from './components/homepage/home-layout/home-layout.component';
import { HomeContentComponent } from './components/homepage/home-content/home-content.component';
import { HomeHeaderComponent } from './components/homepage/home-header/home-header.component';
import { HomeFooterComponent } from './components/homepage/home-footer/home-footer.component';
import { HomeComponent } from './components/homepage/home/home.component';
import { HomeSidebarComponent } from './components/homepage/home-sidebar/home-sidebar.component';
import { LoginComponent } from './components/homepage/home-menu/login/login.component';
import { RegisterComponent } from './components/homepage/home-menu/register/register.component';
import { AboutComponent } from './components/homepage/home-menu/about/about.component';
import { NotFoundComponent } from './components/error-pages/not-found/not-found.component';




@NgModule({
  declarations: [
    AppComponent,
    HomeLayoutComponent,
    HomeContentComponent,
    HomeHeaderComponent,
    HomeFooterComponent,
    HomeComponent,
    HomeSidebarComponent,
    LoginComponent,
    RegisterComponent,
    AboutComponent,
    NotFoundComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    MaterialModule,
    RoutingModule,
    RouterModule,
    FlexLayoutModule,
    FormsModule,
    ReactiveFormsModule,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
