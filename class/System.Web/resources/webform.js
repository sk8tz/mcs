/*
 * WebForm.js
 *
 * Authors:
 *   Chris Toshok (toshok@ximian.com)
 *   Lluis Sanchez Gual (lluis@novell.com)
 *   Igor Zelmanovich (igorz@mainsoft.com)
 *
 * (c) 2005 Novell, Inc. (http://www.novell.com)
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */


function WebForm_AutoFocus (id)
{
	var x = WebForm_GetElementById (id);

	if (x && (!WebForm_CanFocus(x))) {
		x = WebForm_FindFirstFocusableChild(x);
	}
	if (x) { x.focus(); }
}

function WebForm_CanFocus(element) {
	if (!element || !(element.tagName) || element.disabled) {
		return false;
	}
	if (element.type && element.type.toLowerCase() == "hidden") {
		return false;
	}
	var tagName = element.tagName.toLowerCase();
	return (tagName == "input" ||
			tagName == "textarea" ||
			tagName == "select" ||
			tagName == "button" ||
			tagName == "a");
}

function WebForm_FindFirstFocusableChild(element) {
	if (!element || !(element.tagName)) {
		return null;
	}
	var tagName = element.tagName.toLowerCase();
	if (tagName == "undefined") {
		return null;
	}
	var children = element.childNodes;
	if (children) {
		for (var i = 0; i < children.length; i++) {
			try {
				if (WebForm_CanFocus(children[i])) {
					return children[i];
				}
				else {
					var focused = WebForm_FindFirstFocusableChild(children[i]);
					if (WebForm_CanFocus(focused)) {
						return focused;
					}
				}
			} catch (e) {
			}
		}
	}
	return null;
}

function wasControlEnabled (id)
{
	if (typeof (__enabledControlArray) == 'undefined')
		return false;

	for (var i = 0; i < __enabledControlArray.length; i ++) {
		if (id == __enabledControlArray[i])
			return true;
	}

	return false;
}

function WebForm_ReEnableControls (currForm)
{
	currForm = currForm || theForm;
	if (typeof (currForm) == 'undefined')
		return;

	for (var i = 0; i < currForm.childNodes.length; i ++) {
		var node = currForm.childNodes[i];
		if (node.disabled && wasControlEnabled (node.id))
			node.disabled = false;
	}
}

function WebForm_DoPostback (id, par, url, apb, pval, tf, csubm, vg)
{
	var currForm = WebForm_GetFormFromCtrl (id);
	
	if (typeof(SetValidatorContext) == "function") 
		SetValidatorContext (currForm);

	var validationResult = true;
	if (pval && typeof(Page_ClientValidate) == "function")
		validationResult =  Page_ClientValidate(vg);

	if (validationResult) {
		if (url != null)
			currForm.action = url;
	}
		
	if (csubm)
		currForm.__doPostBack (id, par);
}

function WebForm_DoCallback (id, arg, callback, ctx, errorCallback)
{
	var currForm = WebForm_GetFormFromCtrl (id);
	var qs = WebForm_getFormData (currForm) + "__CALLBACKTARGET=" + id + "&__CALLBACKARGUMENT=" + encodeURIComponent(arg);
	if (currForm["__EVENTVALIDATION"]) qs += "&__EVENTVALIDATION=" + encodeURIComponent(currForm["__EVENTVALIDATION"].value);
	WebForm_httpPost (document.URL, qs, function (httpPost) { WebForm_ClientCallback (httpPost, ctx, callback, errorCallback); });
}

function WebForm_ClientCallback (httpPost, ctx, callback, errorCallback)
{
	var doc = httpPost.responseText;
	if (doc.charAt(0) == "e") {
		if ((typeof(errorCallback) != "undefined") && (errorCallback != null))
			errorCallback(doc.substring(1), ctx);
	} else {
		var separatorIndex = doc.indexOf("|");
		if (separatorIndex != -1) {
			var validationFieldLength = parseInt(doc.substring(0, separatorIndex));
			if (!isNaN(validationFieldLength)) {
				var validationField = doc.substring(separatorIndex + 1, separatorIndex + validationFieldLength + 1);
				if (validationField != "") {
					var validationFieldElement = theForm["__EVENTVALIDATION"];
					if (!validationFieldElement) {
						validationFieldElement = document.createElement("INPUT");
						validationFieldElement.type = "hidden";
						validationFieldElement.name = "__EVENTVALIDATION";
						theForm.appendChild(validationFieldElement);
					}
					validationFieldElement.value = validationField;
				}
				if ((typeof(callback) != "undefined") && (callback != null))
					callback (doc.substring(separatorIndex + validationFieldLength + 1), ctx);
			}
		} else {
			if ((typeof(callback) != "undefined") && (callback != null))
				callback (doc, ctx);
		}
	}
}

function WebForm_getFormData (currForm)
{
	var qs = "";
	var len = currForm.elements.length;
	for (n=0; n<len; n++) {
		var elem = currForm.elements [n];
		var tagName = elem.tagName.toLowerCase();
		if (tagName == "input") {
			var type = elem.type;
			if ((type == "text" || type == "hidden" || type == "password" ||
				((type == "checkbox" || type == "radio") && elem.checked)) &&
				(elem.id != "__EVENTVALIDATION")) {
				qs += elem.name + "=" + encodeURIComponent (elem.value) + "&";
			}
		}
		else if (tagName == "select") {
			var selectCount = elem.options.length;
			for (var j = 0; j < selectCount; j++) {
				var selectChild = elem.options[j];
				if (selectChild.selected == true) {
					qs += elem.name + "=" + encodeURIComponent (elem.value) + "&";
				}
			}
		}
		else if (tagName == "textarea") {
			qs += elem.name + "=" + encodeURIComponent (elem.value) + "&";
		}
	}
	return qs;
}

var axName = null;
function WebForm_httpPost (url, data, callback)
{
	var httpPost = null;
	
	if (typeof XMLHttpRequest != "undefined") {
		httpPost = new XMLHttpRequest ();
	} else {
		if (axName != null)
			httpPost = new ActiveXObject (axName);
		else {
			var clsnames = new Array ("MSXML", "MSXML2", "MSXML3", "Microsoft");
			for (n = 0; n < clsnames.length && httpPost == null; n++) {
				axName = clsnames [n] + ".XMLHTTP";
				try {
					httpPost = new ActiveXObject (axName);
				} catch (e) { axName = null; }
			}
			if (httpPost == null)
				throw new Error ("XMLHTTP object could not be created.");
		}
	}
	httpPost.onreadystatechange = function () { if (httpPost.readyState == 4) callback (httpPost); };
	
	httpPost.open ("POST", url, true);	// async
	httpPost.setRequestHeader ("Content-Type", "application/x-www-form-urlencoded");
	setTimeout (function () { httpPost.send (data); }, 10);
}

function WebForm_GetFormFromCtrl (id)
{
	// We need to translate the id from ASPX UniqueID to its ClientID.
	var ctrl = WebForm_GetElementById (id.replace(/\$/g, "_"));
	while (ctrl != null) {
		if (ctrl.isAspForm)
			return ctrl;
		ctrl = ctrl.parentNode;
	}
	return theForm;
}

function WebForm_GetElementById (id)
{
	return document.getElementById ? document.getElementById (id) :
	       document.all ? document.all [id] :
		   document [id];
}

function WebForm_FireDefaultButton(event, target)
{
	if (event.keyCode != 13) {
		return true;
	}
	if(event.srcElement && (event.srcElement.tagName.toLowerCase() == "textarea")) {
		return true;
	}
	var defaultButton = WebForm_GetElementById(target);
	if (defaultButton && typeof(defaultButton.click) != "undefined") {
		defaultButton.click();
		event.cancelBubble = true;
		return false;
	}
	return true;
}

function WebForm_SaveScrollPositionSubmit() {
    this.elements['__SCROLLPOSITIONX'].value = WebForm_GetScrollX();
    this.elements['__SCROLLPOSITIONY'].value = WebForm_GetScrollY();
    if ((typeof(this.oldSubmit) != "undefined") && (this.oldSubmit != null)) {
        return this.oldSubmit();
    }
    return true;
}

function WebForm_SaveScrollPositionOnSubmit() {
    this.elements['__SCROLLPOSITIONX'].value = WebForm_GetScrollX();
    this.elements['__SCROLLPOSITIONY'].value = WebForm_GetScrollY();
    if ((typeof(this.oldOnSubmit) != "undefined") && (this.oldOnSubmit != null)) {
        return this.oldOnSubmit();
    }
    return true;
}

function WebForm_RestoreScrollPosition(currForm) {
	currForm = currForm || theForm;
	var ScrollX = currForm.elements['__SCROLLPOSITIONX'].value;
	var ScrollY = currForm.elements['__SCROLLPOSITIONY'].value;
	if (ScrollX != "" || ScrollY != "")
    	window.scrollTo(ScrollX, ScrollY);
    if ((typeof(this.oldOnLoad) != "undefined") && (this.oldOnLoad != null)) {
        return this.oldOnLoad();
    }
    return true;
}

function WebForm_GetScrollX() {
    if (window.pageXOffset) {
        return window.pageXOffset;
    }
    else if (document.documentElement && document.documentElement.scrollLeft) {
        return document.documentElement.scrollLeft;
    }
    else if (document.body) {
        return document.body.scrollLeft;
    }
    return 0;
}

function WebForm_GetScrollY() {
    if (window.pageYOffset) {
        return window.pageYOffset;
    }
    else if (document.documentElement && document.documentElement.scrollTop) {
        return document.documentElement.scrollTop;
    }
    else if (document.body) {
        return document.body.scrollTop;
    }
    return 0;
}


