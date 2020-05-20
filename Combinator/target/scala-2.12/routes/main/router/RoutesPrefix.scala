
// @GENERATOR:play-routes-compiler
// @SOURCE:/Users/Casper/Documents/Uni/Bachelor/AdventureCLS/Combinator/src/main/resources/routes
// @DATE:Wed May 20 19:12:07 CEST 2020


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
