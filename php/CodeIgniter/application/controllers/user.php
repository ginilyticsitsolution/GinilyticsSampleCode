<?php 
class User extends CI_Controller{
	protected $USER_LANDING_PAGE_URL = 'user/login';

	public function __construct(){
		parent::__construct();
		$this->load->helper('url');
		$this->load->model('user_model');
		$this->load->model('products_model');
		$this->load->library('session');
		$this->load->library('email');
		$this->load->library('form_validation');
	}

   // Registration landing page
	public function index(){
		$this->load->view('layouts/header.php');
		$this->load->view('members/register.php');
		$this->load->view('layouts/footer.php');
	}

	// Sellers registration page (users)
	public function register(){
		$this->form_validation->set_rules('name', 'Name', 'required');
		$this->form_validation->set_rules('email', 'Email', 'trim|required|valid_email|is_unique[users.email]');
		$this->form_validation->set_message('is_unique', 'Email is already Exist!');
		$this->form_validation->set_rules('password', 'Password', 'required|min_length[6]|max_length[8]');

		if ($this->form_validation->run()){
			$this->user_model->register();
			$this->session->set_flashdata('success_msg', 'You have Register Successfully.Please Login');
			redirect($this->USER_LANDING_PAGE_URL);
		}

		$this->load->view('members/register.php');
	}

	// function for login page
	public function login(){
		$this->load->view('layouts/header.php');
		$this->load->view('members/login.php');
		$this->load->view('layouts/footer.php');
	}

   	// function for dashboard
	public function dashboard(){
		$this->form_validation->set_rules('email', 'Email', 'trim|required|valid_email');
		$this->form_validation->set_rules('password', 'Password', 'required|min_length[6]|max_length[8]');
		$isValidForm = $this->form_validation->run();
		$isCredentialsMatched = $this->user_model->matchCredential();

		if($isValidForm && $isCredentialsMatched){
			$email = $this->input->post('email');
			$this->user_model->setUserSession($email);
			$this->home();
		} elseif ($isValidForm && !$isCredentialsMatched){
			$this->session->set_flashdata('error_msg', 'Incorrect username/password.');
			redirect($this->USER_LANDING_PAGE_URL); 
		} else{
			$this->load->view('layouts/header.php');
			$this->load->view('members/login.php');
			$this->load->view('layouts/footer.php');
		}
	}

	//function for logout user
	public function logout(){
		$this->session->sess_destroy();
		redirect($this->USER_LANDING_PAGE_URL);
	}

	// function for home page
	public function home(){
		$data['activeAndVerifiedUserCount'] = $this->user_model->activeUser();
		$data['activeAndVerifiedUserCountWithActiveProduct'] = $this->products_model->activeAndAttachedProduct();
		$data['activeProduct'] = $this->products_model->activeProduct();
		$data['getProductNotAssociatedWithUser'] = $this->products_model->getProductsNotAttachedWithUser();
		$data['getProductAmountOfActiveProduct'] = $this->products_model->getAmountOfActiveAttachedProducts();
		$data['getProductPriceOfActiveProduct'] = $this->products_model->getPriceOfActiveAttachedProducts();
		$data['getProductPriceOfActiveProductForUser'] = $this->products_model->perUserSummarisedPriceOfActiveProducts(); 
		$this->load->view('layouts/header.php');
		$this->load->view('layouts/sidebar.php');
		$this->load->view('members/user_profile.php', $data);
		$this->load->view('layouts/footer.php');
	}

	// function for verify email
	public function verifyEmail()
	{
		$token = $this->input->get('token');
		$email = base64_decode($token);
		$mailExist = $this->user_model->emailCheck($email);

		if(!$mailExist){
			echo "<h2>Invalid Link</h2>";
			return;
		}

		$isVerified = $this->user_model->isVerified($email);

		if($isVerified){
			echo "<h2>User Email is already verified</h2>";
			return;
		} else{
			$this->user_model->emailVerification($email, 1); // 1 is for email verified
			echo "<h2>Your mail Successfully Verified</h2>";
		}

	}
}