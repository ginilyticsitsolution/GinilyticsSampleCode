<div  style="text-align: center; margin: 20px;">
<?php 
  echo $this->session->flashdata('success_msg');
  unset($_SESSION['success_msg']);
?>
</div>

<div class="container">
	<h2>Products</h2>          
	<table class="table table-striped">
		<thead>
			<tr>
				<th>Sr. No.</th>
				<th>Title</th>
				<th>Description</th>
				<th>Image</th>
				<th>status</th>
				<th>Action</th>
			</tr>
		</thead>
			<tbody>
			<?php foreach($products as $products) { ?>
				<tr>
					<td><?php echo $products['id']; ?></td>
					<td><?php echo $products['title']; ?></td>
					<td><?php echo $products['description']; ?></td>
					<td><img  src="<?php echo base_url('user_guide/_images/mobile.jpg');  ?>" style="height:50px;" /></td>
					<td><?php if($products['status'] == 1) { echo "Active"; } else{ echo "Inactive";} ?></td>
					<td><button type="button" class="btn btn-primary" data-toggle="modal" data-id ="<?php echo $products['id']; ?>" id="add_user_product" data-target="#myModal">Add Product</button></td>
				</tr>
			<?php }  ?>
			</tbody>
	</table>
</div>

<!-- The Modal -->
<div class="modal fade" id="myModal">
	<div class="modal-dialog model-center">
		<div class="modal-content">
		<!-- Modal Header -->
		<div class="modal-header">
			<h4 class="modal-title">Attach Product</h4>
			<button type="button" class="close" data-dismiss="modal">&times;</button>
		</div>
		<!-- Modal body -->
		<form role="form" method="post" action="<?php echo base_url('products/addUserProducts'); ?>">
			<div class="modal-body">
			<input type="hidden" class="form-control"name="product_id" id="product_id"/>
				<div class="row form-group">
					<div calss="col-md-4">
						<label><span>Quantity</span></label>
					</div>
					<div calss="col-md-8">
						<input type="number" min='1' class="form-control"name="quantity" id="quantity"/>
					</div> 
				</div>
				<div class="row form-group">
					<div calss="col-md-4">
						<label><span>Price Per Item($)</span></label>
					</div>
					<div calss="col-md-8">
						<input type="number" min='1' class="form-control"name="price" id="price"/>
					</div> 
				</div>
			</div>
			<!-- Modal footer -->
			<div class="modal-footer">
				<button class="btn btn-primary" type="submit" value="userProduct" name="userProduct">Save</button>
			</div>
		</form>
		</div>
	</div>
</div>
<script>
	$( document ).ready(function() {
		$(document).on('click','#add_user_product',function(event){
			let product_id = $(this).data('id'); 
			$('#product_id').val(product_id);
		});
	});
</script>