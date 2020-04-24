
// @GENERATOR:play-routes-compiler
// @SOURCE:/Users/Casper/Documents/Uni/Bachelor/HelloWorldCLS/guidemo/src/main/resources/routes
// @DATE:Tue Apr 07 13:28:48 CEST 2020

import play.api.mvc.Call


import _root_.controllers.Assets.Asset

// @LINE:5
package org.combinators.guidemo {

  // @LINE:5
  class ReverseProductline(_prefix: => String) {
    def _defaultPrefix: String = {
      if (_prefix.endsWith("/")) "" else "/"
    }

  
    // @LINE:7
    def prepare(number:Long): Call = {
      
      Call("GET", _prefix + { _defaultPrefix } + "guidemo/prepare" + play.core.routing.queryString(List(Some(implicitly[play.api.mvc.QueryStringBindable[Long]].unbind("number", number)))))
    }
  
    // @LINE:8
    def serveFile(file:String): Call = {
      
      Call("GET", _prefix + { _defaultPrefix } + "guidemo/guidemo.git/" + implicitly[play.api.mvc.PathBindable[String]].unbind("file", file))
    }
  
    // @LINE:6
    def raw(number:Long): Call = {
      
      Call("GET", _prefix + { _defaultPrefix } + "guidemo/raw_" + play.core.routing.dynamicString(implicitly[play.api.mvc.PathBindable[Long]].unbind("number", number)))
    }
  
    // @LINE:5
    def overview(): Call = {
      
      Call("GET", _prefix + { _defaultPrefix } + "guidemo")
    }
  
  }


}
