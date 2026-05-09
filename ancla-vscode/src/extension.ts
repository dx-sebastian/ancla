import * as vscode from 'vscode';
import { AnclaPanel, AnclaSidebarProvider } from './panelProvider';

export function activate(context: vscode.ExtensionContext): void {
    context.subscriptions.push(
        vscode.commands.registerCommand('ancla.openPanel', () => {
            AnclaPanel.createOrShow(context);
        })
    );

    const sidebarProvider = new AnclaSidebarProvider(context.extensionUri);
    context.subscriptions.push(
        vscode.window.registerWebviewViewProvider(AnclaSidebarProvider.VIEW_ID, sidebarProvider)
    );
}

export function deactivate(): void {}
