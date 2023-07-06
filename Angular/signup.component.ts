import { Component, OnInit } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  FormGroup,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { UserManager } from 'oidc-client';
import { AuthorizeService } from 'src/api-authorization/authorize.service';
import { TYPE } from 'src/app/components/common/enum/enum.component';
import { AccountClient, UserSignUpCommand } from 'src/app/web-api-client';
import { environment } from 'src/environments/environment';
import Swal from 'sweetalert2';
import { LocalService } from '../local.service';
export const url = environment.url;
@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.scss'],
})
export class SignUpComponent implements OnInit {
  submitted = false;
  public signupForm: FormGroup;
  isSubmit=false;
  loading = false;
  singupForm!: FormGroup;
  user: UserSignUpCommand = new UserSignUpCommand();
  faceBookReturnUrl: string = `https://www.facebook.com/v8.0/dialog/oauth?&response_type=token&display=popup&client_id=528467799155434&display=popup&redirect_uri=${url}/signin-facebook&scope=email`;
  googleReturnUrl: string = `${url}/api/Auth/ExternalLogin?provider=Google&returnUrl=${url}/signin-google`;
  constructor(
    private formBuilder: FormBuilder,
    private accountClient: AccountClient,
    private router: Router,
    private authorizeService: AuthorizeService,
    private localStore: LocalService,
  ) { }
  emailValidator(): ValidatorFn {
    const EMAIL_REGEXP = /^(([^<>()\[\]\.,;:\s@\"]+(\.[^<>()\[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/i;
    return (control: AbstractControl): ValidationErrors | null => {
      const isValid = EMAIL_REGEXP.test(control.value);
      if (isValid) {
        return null;
      } else {
        return {
          emailValidator: {
            valid: false,
          },
        };
      }
    };
  };
    numberOnly(event): boolean {

    if(Number.isNaN(event.key))
    return false;
    console.log("Event =>", event);
    const charCode = (event.which) ? event.which : event.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
      return false;
    }
    return true;
  }
  ngOnInit(): void {
    // const data = this.localStore.getData('isUserSignup');
    // const roles = this.localStore.getData('userRolesSignup');
    // if(data.isUserSignup == true && roles.userRolesSignup == "Client"){
    //   this.router.navigate(['/app/my-appointments']);
    // }else if(data.isUserSignup == true && roles.userRolesSignup == "businessOwner"){
    //   this.router.navigate(['business/my-business']);
    // }
    this.createForm();
    this.signupForm = new FormGroup({
      firstName: new FormControl(this.user.email, [
        Validators.required,
        Validators.minLength(1),
        Validators.maxLength(30),
      ]),
      lastName: new FormControl(this.user.lastName, [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(30),
      ]),
      email: new FormControl(this.user.email, [Validators.required,
      Validators.maxLength(60), this.emailValidator()]),
      password: new FormControl(this.user.password, [Validators.required,

      Validators.pattern(/^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,}/)]),
      phone: new FormControl(this.user.phone, [
        Validators.required,
        Validators.minLength(8),
        Validators.maxLength(12),
        Validators.pattern(/^-?(0|[0-9]\d*)?$/),
      ]),
    });
  }

  createForm() {
    this.signupForm = this.formBuilder.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required]],
      password: ['', [Validators.required]],
      phone: ['', [Validators.required]],
    });
  }
  createAccount() {
    this.isSubmit = true;
    if (this.signupForm.invalid) {
      for (const control of Object.keys(this.signupForm.controls)) {
        this.signupForm.controls[control].markAsTouched();
      }
      return;
    }
    this.loading = true;

    this.submitted = true;
    const command = {
      ...this.signupForm.value,
      phone: this.signupForm.controls.phone.value.toString(),
      socialLogin: false,
    } as UserSignUpCommand;
    this.accountClient.signUpUser(command).subscribe((res) => {
      this.signupForm.reset();
      if (res.succeeded == true) {
        this.localStore.saveData('isUserSignup', JSON.stringify({ isUserSignup: true }));
        this.localStore.saveData('userRolesSignup', JSON.stringify({userRolesSignup: "Client"}));
        const roles = this.localStore.getData('userRolesSignup');
        console.log("userRolesSignup1111", roles);
        console.log("Signup1111witharr",roles.userRolesSignup);
        
        
        this.authorizeService.signIn(['/']);
        this.signupForm.reset();
        this.loading = false;
        Swal.fire({
          toast: true,
          position: 'top',
          showConfirmButton: false,
          icon: TYPE.SUCCESS,
          timer: 5000,
          title: 'Signup successfully',
          showCloseButton: true,
          buttonsStyling: true,
        });
        setTimeout(() => {
          this.router.navigate(['/app/my-appointments']);
        }, 300);
      }
    }, (error) => {
      this.loading = false;
    });
  }
  createBusiness() {
    this.isSubmit = true;   
    if (this.signupForm.invalid) {
      for (const control of Object.keys(this.signupForm.controls)) {
        this.signupForm.controls[control].markAsTouched();
      }
      return;
    }
    this.loading = true;
    this.submitted = true;
    const command = {
      ...this.signupForm.value,
      phone: this.signupForm.controls.phone.value.toString(),
      socialLogin: false,
    } as UserSignUpCommand;
    this.accountClient.signUpUser(command).subscribe((res) => {

      if (res.succeeded == true) {
        // this.localStore.saveData('isUserSignup', JSON.stringify({ isUserSignup: true }));
        // this.localStore.saveData('userRolesSignup', JSON.stringify({userRolesSignup: "businessOwner"}));
      //  const rolebase = this.localStore.getData('userRolesSignup');
        // console.log("rolebase", rolebase);
        
        this.authorizeService.signIn(['/']);
        this.signupForm.reset();
        setTimeout(() => {
          this.loading = false;
          this.router.navigate(['/account/signup-business']);
          this.signupForm.reset();
        }, 3000);
      }
    }, (error) => {
      this.loading = false;
    });
  }
  get firstName() {
    return this.signupForm.get('firstName');
  }
  get lastName() {
    return this.signupForm.get('lastName');
  }
  get email() {
    return this.signupForm.get('email');
  }
  get password() {
    return this.signupForm.get('password');
  }
  get phone() {
    return this.signupForm.get('phone');
  }
}
