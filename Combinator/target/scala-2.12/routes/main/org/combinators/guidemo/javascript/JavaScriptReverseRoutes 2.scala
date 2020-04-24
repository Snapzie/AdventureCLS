
// @GENERATOR:play-routes-compiler
// @SOURCE:/Users/Casper/Documents/Uni/Bachelor/HelloWorldCLS/guidemo/src/main/resources/routes
// @DATE:Tue Apr 07 13:28:48 CEST 2020

import play.api.routing.JavaScriptReverseRoute


import _root_.controllers.Assets.Asset

// @LINE:5
package org.combinators.guidemo.javascript {

  // @LINE:5
  class ReverseProductline(_prefix: => String) {

    def _defaultPrefix: String = {
      if (_prefix.endsWith("/")) "" else "/"
    }

  
    // @LINE:7
    def prepare: JavaScriptReverseRoute = JavaScriptReverseRoute(
      "org.combinators.guidemo.Productline.prepare",
      """
        function(number0) {
          return _wA({method:"GET", url:"""" + _prefix + { _defaultPrefix } + """" + "guidemo/prepare" + _qS([(""" + implicitly[play.api.mvc.QueryStringBindable[Long]].javascriptUnbind + """)("number", number0)])})
        }
      """
    )
  
    // @LINE:8
    def serveFile: JavaScriptReverseRoute = JavaScriptReverseRoute(
      "org.combinators.guidemo.Productline.serveFile",
      """
        function(file0) {
          return _wA({method:"GET", url:"""" + _prefix + { _defaultPrefix } + """" + "guidemo/guidemo.git/" + (""" + implicitly[play.api.mvc.PathBindable[String]].javascriptUnbind + """)("file", file0)})
        }
      """
    )
  
    // @LINE:6
    def raw: JavaScriptReverseRoute = JavaScriptReverseRoute(
      "org.combinators.guidemo.Productline.raw",
      """
        function(number0) {
          return _wA({method:"GET", url:"""" + _prefix + { _defaultPrefix } + """" + "guidemo/raw_" + encodeURIComponent((""" + implicitly[play.api.mvc.PathBindable[Long]].javascriptUnbind + """)("number", number0))})
        }
      """
    )
  
    // @LINE:5
    def overview: JavaScriptReverseRoute = JavaScriptReverseRoute(
      "org.combinators.guidemo.Productline.overview",
      """
        function() {
          return _wA({method:"GET", url:"""" + _prefix + { _defaultPrefix } + """" + "guidemo"})
        }
      """
    )
  
  }


}
