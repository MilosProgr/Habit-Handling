import Keycloak from "keycloak-js";

const keycloak = new Keycloak({
  url: "http://localhost:18080/",
  realm: "keycloak-auth-demo",
  clientId: "public-client",
});

export default keycloak;
