<div  style="text-align: center; margin: 20px;">
<?php 
    echo $this->session->flashdata('set_message');
    unset($_SESSION['set_message']);
?>
</div>
<span style="background-color:red;">
    <div class="container"><!-- container class is used to centered  the body of the browser with some decent width-->
        <div class="row"><!-- row class is used for grid system in Bootstrap-->
            <div class="col-md-12 col-md-offset-12"><!--col-md-4 is used to create the no of colums in the grid also use for medimum and large devices-->
                <div class="login-panel panel panel-success col-md-4 col-md-offset-4"  style="top:100px;left:400px;border: 1px solid;border-color: gainsboro;">
                    <div class="panel-heading">
                        <h3 class="panel-title text-center p-2">Sign In</h3>
                    </div>
                    <div class="panel-body">
                        <span class="text-danger"><?php  echo validation_errors(); ?></span>
                        <form role="form" method="post" action="<?php echo base_url('user/dashboard'); ?>">
                            <fieldset>
                                <div class="form-group">
                                    <input class="form-control"   name="email" type="text">
                                </div>
                                <div class="form-group">
                                    <input class="form-control"  name="password" type="password" >
                                </div>
                                <input class="btn btn-lg btn-success btn-block" type="submit" value="login" name="Login" >
                            </fieldset>
                                <span class="text-danger"><?php echo $this->session->flashdata('error_msg');  unset($_SESSION['error_msg']);?></span>
                        </form>
                        <center><b>You dont have account ?</b> <br></b><a href="<?php echo base_url('user/index'); ?>"> Please Register</a></center><!--for centered text-->
                    </div>
                </div>
            </div>
        </div>
    </div>
</span>