// Jquery-5LMT: 5 Levels of Media Type
// Wraps jquery's Ajax with 5LMT implementation to read content type 
// In this case, no need to use JSON.stringify when calling ajax with JSON Object


(function (parent, $) {


    var oldAjax = $.ajax,
        constants = {
            contentTypeName: "Content-Type",
            json: "application/json",
            forms: "x-www-formurlencoded",
            domainModel: "domain-model"
        };

    $.ajax = function (topSettings) {
        topSettings = topSettings || {};
        var originalBeforeSend = topSettings.beforeSend;

        topSettings.beforeSend = function (jqXHR, settings) {
            if (originalBeforeSend)
                originalBeforeSend(jqXHR, settings);
            addFiveLevelsOfMediaType(jqXHR, settings);
        };

        if (topSettings.data && topSettings.dataType && topSettings.dataType.toLowerCase() == "json") {
            topSettings.processData = false;
        }

        oldAjax(topSettings);

    };

    function addFiveLevelsOfMediaType(jqXHR, settings) {

        var contentType, data;
        if (settings.data) {

            if (settings.dataType && settings.dataType.toLowerCase() == "json") {
                data = JSON.stringify(settings.data);
                contentType = constants.json;
            }
            else {
                contentType = constants.forms;
                data = $.params(settings.data);
            }

            // set header and data
            jqXHR.setRequestHeader(constants.contentTypeName,
                contentType + ";" + constants.domainModel + "=" +
               (settings.data.constructor.domainModel || settings.data.constructor.name));
            settings.data = data;

        }

    }

})(window, $);
