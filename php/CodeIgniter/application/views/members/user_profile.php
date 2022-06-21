<style>
.main-content {
    margin-left: 220px;
    padding: 30px;
}
</style>
<div class="main-content">
    <div id="logout">
        <a href="<?php echo base_url('user/logout');?>" >
            <button type="button" class="btn-primary">Logout</button>
        </a>
    </div>
    <div class="row mb-4 ml-2"> 
        <?php echo $this->session->flashdata('success_msg'); ?>
    </div>
    <div class="text-primary ml-2 mb-2"> Verified & Active User Count : <?php echo $activeAndVerifiedUserCount; ?></div>
    <div class="text-primary ml-2 mb-2"> Active and Verified Users count who have attached active products : <?php echo $activeAndVerifiedUserCountWithActiveProduct; ?></div>
    <div class="text-primary ml-2 mb-2"> Count of all active products  : <?php echo $activeProduct; ?></div>
    <div class="text-primary ml-2 mb-2"> Count of active products which don't belong to any user : <?php echo $getProductNotAssociatedWithUser; ?></div>
    <div class="text-primary ml-2 mb-2"> Amount of all active attached products : <?php echo $getProductAmountOfActiveProduct[0]['quantity']; ?></div>
    <div class="text-primary ml-2 mb-6"> Price of all active attached products : <?php echo $getProductPriceOfActiveProduct[0]['price']; ?></div>
    <div class="mt-4">
        <h2 class="mb-4">Prices of all active products per user</h2>          
        <table class="table table-striped">
            <thead>
            <tr class="text-center">
                <th>Sr. No.</th>
                <th>User Name</th>
                <th>Price</th>
                <th>Action</th>
            </tr>
            </thead>
            <tbody>

                <?php foreach($getProductPriceOfActiveProductForUser as $users) { ?>
                    <tr class="text-center">
                        <td><?php echo $users['id']; ?></td>
                        <td><?php echo $users['name']; ?></td>
                        <td><?php echo '€' . $users['price']; ?></td>
                        <td>
                            <button type="button" data-price="<?php echo $users['price']; ?>" class="btn btn-primary exchangeCurrencyModalButton" data-toggle="modal" data-target="#exchangeCurrencyModal">
                            Exchange Currency
                            </button>    
                        </td>
                    </tr>
                <?php }  ?>

            </tbody>
        </table>
    </div>
    <!-- Modal -->
    <div class="modal fade" id="exchangeCurrencyModal" tabindex="-1" role="dialog" aria-labelledby="exchangeCurrencyModal" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered" role="document">
            <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="exampleModalLongTitle">Exchange Currency </h5>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                <span aria-hidden="true">&times;</span>
                </button>
            </div>
            <div class="modal-body">
                <div class="row d-none">
                    <div class="col-md-6 mb-2">
                        <label>Exchange To Currency </label>
                    </div>
                    <div class="col-md-6 mb-2">
                        <select class="form-control" id="currencySelector">
                        <option value="">Select Currency</option>
                        <option value="euro">EURO</option>
                        </select>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6 mb-2">
                        <label>Amount To Exchange (€) </label>
                    </div>
                    <div class="col-md-6 mb-2">
                        <input class="form-control" type="text" id="amountToExchange" value="" readonly />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6 mb-2">
                        <label>Exchanged Amount ($) </label>
                    </div>
                    <div class="col-md-6 mb-2">
                        <input class="form-control" type="text" id="exchangedAmountUSD" value="" readonly />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6 mb-2">
                        <label>Exchanged Amount (lei) </label>
                    </div>
                    <div class="col-md-6 mb-2">
                        <input class="form-control" type="text" id="exchangedAmountRON" value="" readonly />
                    </div>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
                <button type="button" class="btn btn-primary d-none" id="convert">Convert</button>
            </div>
            </div>
        </div>
    </div>
</div>
<script src="https://code.jquery.com/jquery-3.1.1.min.js"> </script>
<script>
    $( document ).ready(function() {
        var config = {
            host: 'http://api.exchangeratesapi.io/v1/',
            access_key: '8288352cbad0fc2ff2a863f005311795'
        };
        pouplateAvailableCurrencyExchangeDropdown();
        $(document).on('click', '.exchangeCurrencyModalButton', function(event){
            $('#amountToExchange').val('');
            $('#exchangedAmountUSD').val('');
            $('#exchangedAmountRON').val('');
            
            let price = $(this).data('price');
            $('#amountToExchange').val(price);
            $('#amountToExchange').data('amount', price);
            
            let toCurrency = $(this).val();
            let amountToExchange = $('#amountToExchange').data('amount'); 
            let url = '&symbols=USD,RON&base=EUR';
             url = `${config.host}latest?access_key=${config.access_key}&symbols=${url}`; 
            $.ajax({
                url : url,
                type : 'GET',
                dataType:'json',
                success : function(response) {
                    if(response.success){
                        // write the code to exchange the currency
                        exchangedAmountUSD =  (amountToExchange*response.rates.USD).toFixed(2)
                        exchangedAmountRON = (amountToExchange*response.rates.RON).toFixed(2);
                        $('#exchangedAmountUSD').val(exchangedAmountUSD);
                        $('#exchangedAmountRON').val(exchangedAmountRON);
                    }             
                },
                error : function(request,error) {}
            });
        });

        function pouplateAvailableCurrencyExchangeDropdown(){
            let url = `${config.host}symbols?access_key=${config.access_key}`;
            $.ajax({
                url,
                type : 'GET',
                dataType:'json',
                success : function(response) {              
                    if(response.success){
                        let symbols = response.symbols;
                        let currencySelectorDropdown = $('#currencySelector');
                        currencySelectorDropdown.children().remove();
                        currencySelectorDropdown.append('<option value="">Select Currency</option>');
                        for(key in symbols){
                            currencySelectorDropdown.append(`<option value="${key}">${symbols[key]}  (${key})</option>`);
                        }
                    }
                    else{
                        alert('Something Went Wrong, Please contact Tech Team.');
                    }
                },
                error : function(request,error) {}
            });
        }
    });
  </script>