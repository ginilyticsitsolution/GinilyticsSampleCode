jQuery(document).ready(function(){
    // Navbar fixed on scroll 
    jQuery(window).scroll(function(){
        var sticky = jQuery('.navbar'),
            scroll = jQuery(window).scrollTop();

        if (scroll >= 50) sticky.addClass('navbar-stick');
        else sticky.removeClass('navbar-stick');
    });
    

    //Metting notification close 
    jQuery(".notificaation a.align-middle").click(function(){
        jQuery(".notificaation").hide();
    })

    //Floating buttons
    jQuery(".floating-btn").click(function(){
        jQuery(".floating-label").toggleClass("open");
    });

    //Floating buttons - click outside the box
    jQuery(document).click(function (e) {
        if (!jQuery(e.target).hasClass("floating-label ") && jQuery(e.target).parents(".floating-btn").length === 0 && jQuery("#menu-share").hasClass('open')) {
            jQuery("#menu-share").trigger('click');
        }
    });;

    // TR Edit 
    jQuery(".edit-row").click(function () {
        jQuery(".edit-row").hide();
    });
    //TR edit row
    jQuery(".edit-row").click(function () {
        jQuery(".tr-edit-row").show("fast");
    });
    jQuery(".close-tr-edit-row").click(function () {
        jQuery(".tr-edit-row").hide("fast");
        jQuery(".edit-row").show();
    });
});