// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
	console.log("Tuja Esploristo Ajax preta")	

	var click = 0;

	$.support.cors = true;
	$('#clickJson').click(function () {
		console.log("clickJson");	
		let path = $('input.form-control').val();
		$('.tujaMessage').text("Je calcule, ..." );
		$.ajax({
			Type: "GET",
			url: "statutoJson",
			crossDomain: true,
			contentType: "application/json;charset=utf-8",
			data: { "path": path },
			processData: true,
			cache: false,
			dataType: 'json',
			success: function (data, status, xhr) {
				console.log("clickJson success");	
				$('.tujaMessage').text("Résultat = " + JSON.stringify(data) );
				//alert(JSON.stringify(data));
			},         //xhr = xml http request
			Error: function (xhr, status, error) {
				console.log("clickJson Error");	
				alert(error);
				},
		});

	});

	$('#clickHtml').click(function () {
		console.log("clickHtml");
		$('.tujaMessage').text("Je calcule, ...");
		let path = $('input.form-control').val();
		let $div = $('.clickHtml');
		$.get('statutoHtml', { "path": path }, function (data) {
			$div.html(data);
		});
	});
	$('#utf8xml').click(function () {
		console.log("utf8xml");
		click++;
		if ($('#utf8xml').hasClass("btn-success")) {
			$('#utf8xml').toggleClass("btn-success");
			$('#utf8xml').toggleClass("btn-danger");
			$('.tujaMessage').text("utf8xml, Je calcule, ..." + click);
			$('.clickHtml').html("");
			let path = $('input.form-control').val();
			let $div = $('.clickHtml');
			$.post('utf8xml', { "path": path }, function (data) {
				$('#utf8xml').toggleClass("btn-danger");
				$('#utf8xml').toggleClass("btn-success");
				$div.html(data);
			});
		}
	});
	$('#vortaroj').click(function () {
		console.log("vortaroj");
		click++;
		if ($('#vortaroj').hasClass("btn-success")) {
			$('#vortaroj').toggleClass("btn-success");
			$('#vortaroj').toggleClass("btn-danger");
			$('.tujaMessage').text("vortaroj, Je calcule, ..." + click);
			$('.clickHtml').html("");
			let path = $('input.form-control').val();
			let $div = $('.clickHtml');
			$.post('vortaroj', { "path": path }, function (data) {
				$('#vortaroj').toggleClass("btn-danger");
				$('#vortaroj').toggleClass("btn-success");
				$div.html(data);
			});
		}
	});
});