$(function () {
    $.validator.setDefaults({
        ignore: ""
    });

    $.validator.addMethod("requiredif", function (value, element, params) {
        if ($(element).val() != '') return true;

        var $other = $('#' + params.other);

        var otherVal = (typeof $other.attr('type') !== "undefined" && $other.attr('type').toUpperCase() == "CHECKBOX") ?
					   ($other.attr("checked") ? "true" : "false") : $other.val();

		return params.comp == 'isequalto' ? (otherVal != params.value)
										  : (otherVal == params.value);
	});

    $.validator.unobtrusive.adapters.add("requiredif", ["other", "comp", "value"],
		function (options) {
			options.rules['requiredif'] = {
				other: options.params.other,
				comp: options.params.comp,
				value: options.params.value
			};
			options.messages['requiredif'] = options.message;
		}
	);
});