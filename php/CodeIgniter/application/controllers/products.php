<?php
class Products extends CI_Controller{
    protected $PRODUCT_LANDING_PAGE_URL = 'products/index';

    public function __construct(){
        parent::__construct();
        $this->load->helper('url');
        $this->load->model('products_model');
        $this->load->library('session');
        $this->load->library('form_validation');
    }

    // function for to show products list
    public function index(){
        $data['products'] = $this->products_model->products();
        $this->load->view('layouts/header.php');
        $this->load->view('layouts/sidebar.php');
        $this->load->view('members/products.php', $data);
        $this->load->view('layouts/footer.php');
    }

    //function for add product and price by user
    public function addUserProducts(){
        $this->form_validation->set_rules('quantity', 'quantity', 'required');
        $this->form_validation->set_rules('price', 'price', 'required');
        $this->form_validation->set_rules('product_id', 'product_id', 'required',
            ['required' => 'You must provide a %s.']
        );

        if ($this->form_validation->run()) {
           return $this->createProduct();
        }

        $this->session->set_flashdata('error_msg', 'Error occured, Try again.');
        redirect($this->PRODUCT_LANDING_PAGE_URL);
    }

    public function createProduct()
    {
        $userEmail = $this->session->userData('userEmail');
        $userData = $this->products_model->getUserByEmail($userEmail);
        $userId = $userData->id;
        $userProduct = [
            'quantity' => $this->input->post('quantity'),
            'price' => $this->input->post('price'),
            'product_id' => $this->input->post('product_id'),
            'user_id' => $userId
        ];
        $this->products_model->insertUserProduct($userProduct);
        $this->session->set_flashdata('success_msg', 'Product Added successfully.');
        return redirect($this->PRODUCT_LANDING_PAGE_URL);
    }
}