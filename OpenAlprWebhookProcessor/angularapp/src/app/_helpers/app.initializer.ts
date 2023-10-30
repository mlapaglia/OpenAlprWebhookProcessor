import { AccountService } from "app/_services";
import { TeardownLogic } from "rxjs";

export function appInitializer(accountService: AccountService) {
    return () => new Promise(resolve => {
        // attempt to refresh token on app start up to auto authenticate
        accountService.refreshToken()
            .subscribe()
            .add(resolve as TeardownLogic);
    });
}