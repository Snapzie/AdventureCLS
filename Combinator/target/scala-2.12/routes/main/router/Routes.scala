
// @GENERATOR:play-routes-compiler
// @SOURCE:/Users/Casper/Documents/Uni/Bachelor/AdventureCLS/Combinator/src/main/resources/routes
// @DATE:Sun Apr 19 17:12:28 CEST 2020

package router

import play.core.routing._
import play.core.routing.HandlerInvokerFactory._

import play.api.mvc._

import _root_.controllers.Assets.Asset

class Routes(
  override val errorHandler: play.api.http.HttpErrorHandler, 
  // @LINE:2
  org_combinators_cls_git_Routes_0: org.combinators.cls.git.Routes,
  // @LINE:3
  org_combinators_guidemo_AdventureGameVersion1Controller_1: org.combinators.guidemo.AdventureGameVersion1Controller,
  val prefix: String
) extends GeneratedRouter {

   @javax.inject.Inject()
   def this(errorHandler: play.api.http.HttpErrorHandler,
    // @LINE:2
    org_combinators_cls_git_Routes_0: org.combinators.cls.git.Routes,
    // @LINE:3
    org_combinators_guidemo_AdventureGameVersion1Controller_1: org.combinators.guidemo.AdventureGameVersion1Controller
  ) = this(errorHandler, org_combinators_cls_git_Routes_0, org_combinators_guidemo_AdventureGameVersion1Controller_1, "/")

  def withPrefix(prefix: String): Routes = {
    router.RoutesPrefix.setPrefix(prefix)
    new Routes(errorHandler, org_combinators_cls_git_Routes_0, org_combinators_guidemo_AdventureGameVersion1Controller_1, prefix)
  }

  private[this] val defaultPrefix: String = {
    if (this.prefix.endsWith("/")) "" else "/"
  }

  def documentation = List(
    prefixed_org_combinators_cls_git_Routes_0_0.router.documentation,
    prefixed_org_combinators_guidemo_AdventureGameVersion1Controller_1_1.router.documentation,
    Nil
  ).foldLeft(List.empty[(String,String,String)]) { (s,e) => e.asInstanceOf[Any] match {
    case r @ (_,_,_) => s :+ r.asInstanceOf[(String,String,String)]
    case l => s ++ l.asInstanceOf[List[(String,String,String)]]
  }}


  // @LINE:2
  private[this] val prefixed_org_combinators_cls_git_Routes_0_0 = Include(org_combinators_cls_git_Routes_0.withPrefix(this.prefix + (if (this.prefix.endsWith("/")) "" else "/") + ""))

  // @LINE:3
  private[this] val prefixed_org_combinators_guidemo_AdventureGameVersion1Controller_1_1 = Include(org_combinators_guidemo_AdventureGameVersion1Controller_1.withPrefix(this.prefix + (if (this.prefix.endsWith("/")) "" else "/") + ""))


  def routes: PartialFunction[RequestHeader, Handler] = {
  
    // @LINE:2
    case prefixed_org_combinators_cls_git_Routes_0_0(handler) => handler
  
    // @LINE:3
    case prefixed_org_combinators_guidemo_AdventureGameVersion1Controller_1_1(handler) => handler
  }
}
