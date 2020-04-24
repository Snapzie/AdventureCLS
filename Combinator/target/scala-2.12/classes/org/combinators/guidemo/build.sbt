name := "CustomerForm"
version := "1.0"

libraryDependencies ++= Seq(
  "com.fasterxml.jackson.core" % "jackson-annotations" % "2.8.8",
  "com.fasterxml.jackson.core" % "jackson-core" % "2.8.8",
  "com.fasterxml.jackson.core" % "jackson-databind" % "2.8.8"
)

mainClass in (Compile, run) := Some("org.combinators.guidemo.CustomerForm")