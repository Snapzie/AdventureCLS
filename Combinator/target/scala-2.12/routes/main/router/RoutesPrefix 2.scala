
// @GENERATOR:play-routes-compiler
// @SOURCE:/Users/karllevinsen/Documents/GitHub/bachelor/AdventureCLS/Combinator/src/main/resources/routes
// @DATE:Sat May 23 11:53:24 CEST 2020


package router {
  object RoutesPrefix {
    private var _prefix: String = "/"
    def setPrefix(p: String): Unit = {
      _prefix = p
    }
    def prefix: String = _prefix
    val byNamePrefix: Function0[String] = { () => prefix }
  }
}
