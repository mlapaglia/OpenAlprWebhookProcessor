import { AccountService } from "@app/_services";

export function appInitializer(authenticationService: AccountService) {
    return () => new Promise(resolve => {
        authenticationService.refreshToken()
            .subscribe()
            .add(resolve);
    });
}