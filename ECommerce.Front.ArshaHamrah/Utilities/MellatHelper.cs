﻿using System.Collections.Specialized;
using System.Text;

namespace ECommerce.Front.ArshaHamrah.Utilities;

public static class MellatHelper
{
    public static string PreparePOSTForm(string url, NameValueCollection data)
    {
        //Set a name for the form
        var formID = "PostForm";

        //Build the form using the specified data to be posted.
        var strForm = new StringBuilder();
        strForm.Append("<form id=\"" + formID + "\" name=\"" + formID + "\" action=\"" + url + "\" method=\"POST\">");
        foreach (string key in data)
            strForm.Append("<input type=\"hidden\" name=\"" + key + "\" value=\"" + data[key] + "\">");
        strForm.Append("</form>");

        //Build the JavaScript which will do the Posting operation.
        var strScript = new StringBuilder();
        strScript.Append("<script language='javascript'>");
        strScript.Append("var v" + formID + " = document." + formID + ";");
        strScript.Append("v" + formID + ".submit();");
        strScript.Append("</script>");

        //Return the form and the script concatenated. (The order is important, Form then JavaScript)
        return strForm + strScript.ToString();
    }
}