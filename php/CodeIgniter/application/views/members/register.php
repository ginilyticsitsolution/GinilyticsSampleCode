<!DOCTYPE html>
<html>
    <head>
        <meta charset="utf-8">
        <title>Login Registration</title>
        <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css" media="screen" title="no title">
        <script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.2.1/jquery.js"></script>
    </head>
    <body>
        <span style="background-color:red;">
            <div class="container"  style="margin-top:150px;">
                <div class="row">
                    <div class="col-md-4 col-md-offset-4">
                        <div class="login-panel panel panel-success">
                            <div class="panel-heading">
                                <h3 class="panel-title">Please do Registration here</h3>
                            </div>
                            <div class="panel-body">
                                <span class="text-danger"><?php echo validation_errors();?></span>
                                <form role="form" method="post" action="<?php echo base_url('user/register'); ?>">
                                    <fieldset>
                                        <div class="form-group">
                                            <input class="form-control" placeholder="Please enter Name" name="name" type="text" >
                                        </div>
                                        <div class="form-group">
                                            <input class="form-control" placeholder="Please enter E-mail" name="email" type="text" >
                                        </div>
                                        <div class="form-group">
                                            <input class ="form-control" placeholder="Enter Password" name="password" type="password" >
                                        </div>
                                        <div class="form-group">
                                            <select class="form-control" name ="role">
                                                <option value="1">Admin</option>
                                                <option value="2">User</option>
                                            </select>
                                        </div>
                                        <button class="btn btn-lg btn-success btn-block btn-submit" value="Register" name="register" >Register</button>
                                    </fieldset>
                                </form>
                                <center><b>You have Already registered ?</b> <br></b><a href="<?php echo base_url('user/login'); ?>"> Please Login</a></center>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </span>
    </body>
</html>
