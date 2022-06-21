<?php
    class Products_model extends CI_model{
        public function __construct() {
            parent::__construct();
            $this->load->model('User_model');
        }
        public $active = 1;    // status 1 = active product status 2 =  deactive product
        
        //get all products
        public function products(){
            $this->db->select('*');
            $this->db->from('products');
            $query = $this->db->get();
            return ($query) ? $query->result_array() : null;
        }

        //insert user product
        public function insertUserProduct($user){
            $this->db->insert('user_products', $user);
        }

        public function getUserByEmail($userMail){
            $this->db->select('*');
            $this->db->from('users');
            $this->db->where('email', $userMail);
            $query = $this->db->get();
            return ($query) ? $query->row() : null;
        }

        // get active and attached product
        public function activeAndAttachedProduct(){
            $this->db->select('*')
            ->from('user_products')
            ->join('users', 'users.id = user_products.user_id')
            ->join('products', 'products.id = user_products.product_id')
            ->where('products.status', $this->active)
            ->where('users.status', $this->User_model->active);
            
            $query = $this->db->get();
            
            return ($query) ? $query->num_rows() : null;
        }

        // get all active product
        public function activeProduct(){
            $this->db->select('*')
            ->from('products')
            ->where('status', $this->active);
            $query = $this->db->get();
            return ($query) ? $query->num_rows() : null;
        }

        // get product those not attached with user
        public function getProductsNotAttachedWithUser(){
            $sql = "SELECT * from products where id NOT IN ( SELECT product_id FROM `products` JOIN `user_products` ON `user_products`.`product_id` = `products`.`id`)"; 
            $query = $this->db->query($sql);
            return ($query) ? $query->num_rows() : null;
        }

        // get amount of all active attached product by user
        public function getAmountOfActiveAttachedProducts(){
            $this->db->select('*')
            ->from('products')->join('user_products', 'products.id = user_products.product_id')
            ->where('products.status', $this->active);
            $this->db->select_sum('quantity');
            $query = $this->db->get();
            return ($query) ? $query->result_array() : null;
        }

        // get price of all active and attached product
        public function getPriceOfActiveAttachedProducts(){
            $this->db
            ->select('*')
            ->from('products')
            ->join('user_products', 'products.id = user_products.product_id')
            ->where('products.status', $this->active);
            $this->db->select_sum('price');
            $query = $this->db->get();
            return ($query) ? $query->result_array() : null;
        }

        // get price of product according to user
        public function perUserSummarisedPriceOfActiveProducts(){
            $this->db->select('*', 'user_products.price');
            $this->db->from('users')->join('user_products', 'user_products.user_id = users.id');
            $this->db->where('users.status', $this->User_model->active);
            $this->db->group_by('user_products.user_id');
            $this->db->select_sum('user_products.price');
            $query = $this->db->get();
            return ($query) ? $query->result_array() : null;   
        }
    }
?>