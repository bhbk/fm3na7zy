﻿using System;

namespace Bhbk.Lib.Aurora.Domain.Primitives
{
    public class Templates
    {
        #region Templates

        /*
         * https://htmlformatter.com
         * https://www.freeformatter.com/java-dotnet-escape.html
         */

        public static string NotifyEmailOnFileUpload(string serverName, string userName, string firstName, string lastName, string fullPath, string bytesTransferred)
        {
            /*
             * use http://rendera.herokuapp.com/ to test template before format...
             * use https://www.buildmystring.com to format template into string that compiles...
             */

            return "<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\" \"http://www.w3.org/TR/REC-html40/loose.dtd\">" +
            "<html xmlns=\"http://www.w3.org/1999/xhtml\">" +
            "  <head>" +
            "    <!--[if !mso]><!-- -->" +
            "    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">" +
            "    <style>" +
            "      @font-face { font-family: Open Sans; src: url('http://fonts.googleapis.com/css?family=Open+Sans'); }" +
            "    </style>" +
            "    <!--<![endif]-->" +
            "    <style>" +
            "      table { color: inherit; }" +
            "    </style>" +
            "  </head>" +
            "  <body style=\"font-size: 31px; font-family: 'Open Sans', 'HelveticaNeue-Light', 'Helvetica Neue Light', 'Helvetica Neue', Helvetica, Arial, 'Lucida Grande', sans-serif; color:#404040; padding: 0; width: 100% !important; -webkit-text-size-adjust: 100%; font-weight: 300 !important; margin: 0; -ms-text-size-adjust: 100%;\" mar=g inheight=\"0\" marginwidth=\"0\" id=\"dbx-email-body\">" +
            "    <div style=\"max-width: 600px !important; padding: 4px;\">" +
            "      <table cellpadding=\"0\" cellspacing=\"0\" style=\"padding: 0 45px; width: 100% !important; padding-top: 45px;border: 1px solid #F0F0F0; background-color: #FFFFFF;\" border=\"0\" align==\"center\">" +
            "        <tr>" +
            "          <td align=\"center\">" +
            "            <table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" width=\"100%\">" +
            "              <tr style=\"font-size: 16px; font-weight: 300; color: #404040; font-family: 'Open Sans', 'HelveticaNeue-Light', 'Helvetica Neue Light', 'Helvetica Neue', Helvetica, Arial, 'Lucida Grande', sans-serif; line-height: 26px; text-align: left;\">" +
            "                <td>" +
            "                  <br>" +
            "                  <br>Hi " + string.Format("{0} {1}", firstName, lastName) + "." +
            "                  <br>" +
            "                  <br>The user " + userName + " accessed " + serverName + " via SFTP." +
            "                  <br>The file /" + fullPath + " uploaded is " + bytesTransferred + " bytes." +
            "                  <br>" +
            "                  <br>The data within will be processed without any more humans or files involved now. Thank-you." +
            "                  <br>" +
            "                </td>" +
            "              </tr>" +
            "              <tr>" +
            "                <td height=\"40\"></td>" +
            "              </tr>" +
            "            </table>" +
            "          </td>" +
            "        </tr>" +
            "      </table>" +
            "    </div>" +
            "  </body>" +
            "</html>";
        }

        public static string NotifyTextOnFileUpload(string serverName, string userName, string fullPath, string bytesTransferred)
        {
            return $"The user {userName} accessed {serverName} via SFTP. " + Environment.NewLine
                + $"The file /{fullPath} uploaded is {bytesTransferred} bytes. " + Environment.NewLine
                + $"The data within will be processed without any more humans or files involved now. Thank-you. ";
        }

        #endregion
    }
}
