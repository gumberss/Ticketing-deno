import { Listener } from './Listener'
import { Message } from 'amqplib'
import { Exchanges } from '../exchanges'
import { ProductCreatedEvent } from '../events/ProductCreatedEvent'
import { Product } from '../../../models/product'
import { Stock } from '../../../models/stock'

export class ProductCreatedListener extends Listener<ProductCreatedEvent> {
	readonly exchange = Exchanges.ProductCreated

	constructor(queueName: string) {
		super(queueName)
	}

	async consume(content: ProductCreatedEvent, msg: Message): Promise<void> {
		const { id, title, description, price } = content

		console.log(content.id)

		const product = Product.build({
			id,
			title,
			description,
			price,
		})

		await product.save()

		const stock = Stock.build({
			quantity: 0,
			availableQuantity: 0,
			product: product
		})
		
		await stock.save()

		console.log(`Product created: ${product._id}`)
	}
}
