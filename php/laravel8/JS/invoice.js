<script type="text/javascript">
	$(document).ready(function(){
		//declared them as global varibale because these variables are going to be used by many functions but the value will be set only by one function, which is when "contextMenu" gets opened.
		var currentlyClickedFileLink = "";
		var currentlyClickedAttachmentId = "";
		var currentlyClickedAttachmentName = "";

		lightbox.option({
			albumLabel:'Image %1 of %2',
			alwaysShowNavOnTouchDevices:false,
			fadeDuration: 600,
			fitImagesInViewport:true,
			imageFadeDuration: 600,
			showImageNumberLabel:false,
		})

		$('#facility_id').select2();
		$('#invoiceYear').select2();

		$(document).on('click', '.invoicesDownload', function(e){
			let url = $(this).children('span').data('filedownloadlink');
			window.open(url, "_self");
		});

		$(document).on('change', '.invoicesAttachments', function(e){
			let formData = new FormData();
			let TotalFiles = $(this)[0].files.length; //Total files
			let files = $(this)[0];
			for (let index = 0; index < TotalFiles; index++) {
				formData.append('files' + index, files.files[index]);
			}

			let selectedMonth = $(this).closest('.invoiceUploadList').find('input[name="month_sequence"]').val();
			let selectedYear = $("#invoiceYear").val();

			formData.append('total_files', TotalFiles);
			formData.append('selectedYear', selectedYear);
			formData.append('selectedMonth', selectedMonth);

			$.ajax({
				url: "{{route('invoices.store')}}",
				type: 'post',
				data: formData,
				contentType: false,
				processData: false,
				success: function(response) {
					if(response.status === true){
						toastr.success(response.message, 'Success', { timeOut: 1000, fadeOut: 1000,
							onHidden: function () {
								window.location.reload(); 
							}
						});
					}
					else{
						toastr.error(response.message, 'Error', { timeOut: 1000, fadeOut: 1000});
					}
				},
				error: function(response) {
					let errors = response.responseJSON.errors;
					$.each(errors, function (index, value) {
						toastr.error(value[0], 'Error', { timeOut: 1000, fadeOut: 1000});
					});
				}
			});
		})

		$(document).on('click', '.invoiceUpload', function(e){
			$(this).siblings('.invoicesAttachments').trigger('click');
		})

		$('#facility_id').change(function(){
			var id = $(this).val();
			updateSelectedFacilityForUsers(id);
		});

		$('#invoiceYear').on("change", function(e) {
			let url = "{{route('invoices.index')}}?selectedYear=" + $(this).val();
			window.location.assign(url);
		});

		$(document).on('click', function(e){
			if(e.button == 0){
				hideContextMenu();
			}
		})

		// Trigger action when the contexmenu is about to be shown
		$(document).bind("contextmenu", function (event) {
			currentlyClickedFileLink = $(event.target).data('href');
			currentlyClickedAttachmentId = $(event.target).data('id');
			currentlyClickedAttachmentName = $(event.target).data('name');
			// Avoid the real one

			if ($(event.target).parents(".viewFileInNewTab").length == 0) {
				hideContextMenu(); //Hide it
			}
			else{
				event.preventDefault();
				//check if the context menu is already open iy yes then do nothing
				if($(".custom-menu").eq(0).is(':visible'))
					hideContextMenu();

				// Show contextmenu
				$(".custom-menu").finish().toggle(100).css({
					top: event.pageY + "px",
					left: event.pageX + "px"
				});
			}
		});

		// If the menu element is clicked
		$(document).on("click", '.custom-menu li', function (event) {
			// This is the triggered action name
			let action = $(this).data("action");

			switch(action) {
				case "view": 
				if(currentlyClickedFileLink)
					window.open(currentlyClickedFileLink, "_blank");
				break;
				
				case "download":
				if(currentlyClickedFileLink)
					download(currentlyClickedFileLink, currentlyClickedAttachmentName);
				break;
				
				case "delete":
				if(currentlyClickedAttachmentId)
					deleteInvoiceAttachmentConfirmation(currentlyClickedAttachmentId);
				break;
				
				case "rename":
				$("#renameAttachmentButton").trigger('click');
				break;
			}
			// Hide it AFTER the action was triggered
			$(".custom-menu").hide(100);
		});

		function updateSelectedFacilityForUsers(id) {
			let reloadLocationUrl = "{{route('invoices.index')}}" + `?selectedYear={{$selectedYear}}`;
			$.ajax({
				url: "{{route('user.update_selected_facility')}}",
				type: 'post',
				data: {'facility_id': id},
				success: function(response) {
					if(response.status === true){
						window.location.href = reloadLocationUrl;
					}
					else{
						toastr.error(response.message, 'Error', { timeOut: 1000, fadeOut: 1000});
					}
				},
				error: function(response) {
					let errors = response.responseJSON.errors;
					$.each(errors, function (index, value) {
						toastr.error(value[0], 'Error', { timeOut: 1000, fadeOut: 1000});
					});
				}
			});
		}

		function hideContextMenu(){
			$(".custom-menu").hide(100)
		}

		function deleteInvoiceAttachmentConfirmation(id){
			bootbox.confirm({
				message: "Do you really want to delete this attachment?",
				buttons: {
					confirm: {
						label: 'Yes',
						className: 'btn-success'
					},
					cancel: {
						label: 'No',
						className: 'btn-danger'
					}
				},
				callback: function (result) {
					if(result)	deleteInvoiceAttachmentById(id)
				}
		});
		}
	});
</script>