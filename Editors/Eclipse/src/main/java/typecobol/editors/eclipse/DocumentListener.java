package typecobol.editors.eclipse;

import typecobol.client.CodeElement;
import typecobol.client.TokensProvider;
import typecobol.client.Parser;

import java.util.List;

import org.eclipse.jface.text.*;

public class DocumentListener implements IDocumentListener, IDocumentPartitioningListener, TokensProvider {

	private final Parser parser = new Parser();
	private final MarkerCreator handler;

	public DocumentListener(final MarkerCreator handler) {
		this.handler = handler;
	}

	@Override
	public void documentAboutToBeChanged(final DocumentEvent event) { }

	@Override
	public void documentChanged(final DocumentEvent event) {
		parser.parse(event.fDocument.get());

		if (parser.elements != null) {
			for (final CodeElement e: parser.elements) {
				if (e.errors.size() < 1) continue;

				int offset = -1;
				try { offset = event.fDocument.getLineOffset(e.lineFirst); }
				catch (final BadLocationException ex) { }
				handler.error(e, offset);
			}
		}
		//else ;//TODO error marker parsing failed
	}

	@Override
	public void documentPartitioningChanged(final IDocument document) { }

	@Override
	public List<typecobol.client.Token> getTokens() {
		final List<typecobol.client.Token> tokens = new java.util.ArrayList<typecobol.client.Token>();
		if (parser.elements == null) return tokens;
		for (final CodeElement e: parser.elements) tokens.addAll(e.tokens);
		return tokens;
	}

}
