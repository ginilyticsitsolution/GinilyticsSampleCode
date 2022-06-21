<?php 
    class User_model extends CI_model{
        public function __construct() {
            parent::__construct();
            $this->load->model('Products_model');
        }
        public $active = 'active';
        
        // server side validation to check email exist or not
        public function emailCheck($email){
            $this->db->select('*');
            $this->db->from('users');
            $this->db->where('email', $email);
            $query = $this->db->get();
            return ($query) ? $query->row() : null;
        }

        public function isVerified($email){
            $this->db->select('*');
            $this->db->from('users');
            $this->db->where('email', $email);
            $query = $this->db->get();
            $user = $query->row();
            return $user->is_email_verified == $this->Products_model->active; //1 means user email is verified
        }

        public function matchCredential() {
            $this->db->select('*');
            $this->db->from('users');
            $this->db->where('email', $this->input->post('email'));
            $this->db->where('password', md5($this->input->post('password')));
            $query = $this->db->get();
            return $query->num_rows() > 0;
        }

        public function emailVerification($email, $value)
        {
            $update_rows = [
                'is_email_verified' => $this->Products_model->active
            ];
            $this->db->where('email', $email );
            $result = $this->db->update('users', $update_rows);
            return $result;
        }

        // save user data in users table
        public function register(){
            $data = [
                'name' => $this->input->post('name'),
                'password' => md5($this->input->post('password')),
                'email' => $this->input->post('email')
            ];
            $this->db->insert('users', $data);
        }

        // function for get active users
        public function activeUser(){
            $this->db->select('*');
            $this->db->from('users');
            $this->db->where('status', $this->active);
            $this->db->where('is_email_verified', $this->Products_model->active);
            $query = $this->db->get();
            return ($query) ? $query->num_rows() : null;
        }

        // set user data in session
        public function setUserSession($email){
            $this->db->select('*');
            $this->db->from('users');
            $this->db->where('email', $email);
            $query = $this->db->get();
            $user = $query->row();

            if(empty($user)){
                return false;
            }

            $this->session->set_userData([
                'userId'  => $user->id,
                'userEmail'  => $user->email
            ]);
            return $user ? true : false;
        }
    }
?>