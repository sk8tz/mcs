/* -*- Mode: C++; tab-width: 2; indent-tabs-mode: nil; c-basic-offset: 2 -*- */
/* ***** BEGIN LICENSE BLOCK *****
 * Version: MPL 1.1/GPL 2.0/LGPL 2.1
 *
 * The contents of this file are subject to the Mozilla Public License Version
 * 1.1 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the
 * License.
 *
 * The Original Code is Mozilla Communicator client code, released
 * March 31, 1998.
 *
 * The Initial Developer of the Original Code is
 * Netscape Communications Corporation.
 * Portions created by the Initial Developer are Copyright (C) 1998
 * the Initial Developer. All Rights Reserved.
 *
 * Contributor(s):
 *
 * Alternatively, the contents of this file may be used under the terms of
 * either the GNU General Public License Version 2 or later (the "GPL"), or
 * the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
 * in which case the provisions of the GPL or the LGPL are applicable instead
 * of those above. If you wish to allow use of your version of this file only
 * under the terms of either the GPL or the LGPL, and not to allow others to
 * use your version of this file under the terms of the MPL, indicate your
 * decision by deleting the provisions above and replace them with the notice
 * and other provisions required by the GPL or the LGPL. If you do not delete
 * the provisions above, a recipient may use your version of this file under
 * the terms of any one of the MPL, the GPL or the LGPL.
 *
 * ***** END LICENSE BLOCK ***** */

/**
   File Name:          7.4.2-7-n.js
   ECMA Section:       7.4.2

   Description:
   The following tokens are ECMAScript keywords and may not be used as
   identifiers in ECMAScript programs.

   Syntax

   Keyword :: one of
   break          for         new         var
   continue       function    return      void
   delete         if          this        while
   else           in          typeof      with

   This test verifies that the keyword cannot be used as an identifier.
   Functioinal tests of the keyword may be found in the section corresponding
   to the function of the keyword.

   Author:             christine@netscape.com
   Date:               12 november 1997

*/
var SECTION = "7.4.2-7";
var VERSION = "ECMA_1";
startTest();
writeHeaderToLog( SECTION + " Keywords");

DESCRIPTION = "var return = true";
EXPECTED = "error";

new TestCase( SECTION,  "var return = true",     "error",    eval("var return = true") );

test();
